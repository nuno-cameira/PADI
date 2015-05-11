using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Padi.SharedModel;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.IO;
using System.Threading;

namespace Client
{
    public class Client : MarshalByRefObject, IClient
    {
        private static readonly string ABORT_MESSAGE = "Aborting submition...";

        //Local worker node used to submit jobs
        private IWorker localWorker = null;

        private readonly TcpChannel channel = null;
        private readonly string url = null;

        private string inputPath = null;
        private string outputPath = null;

        private bool job = false;

        public struct SplitInfo
        {
            public int pos, length;

            public SplitInfo(int p, int l)
            {
                pos = p;
                length = l;
            }
        }

        private Dictionary<int, SplitInfo> splitsDictionary = new Dictionary<int, SplitInfo>();



        public Client(string EntryURL, int Port)
        {
            Console.WriteLine("Creating the Client...");

            this.channel = new TcpChannel(Port);
            this.url = "tcp://" + Util.LocalIPAddress() + ":" + Port + "/C";

            ChannelServices.RegisterChannel(this.channel, false);
            RemotingServices.Marshal(this, "C", typeof(Client));


            this.localWorker = (IWorker)Activator.GetObject(typeof(IWorker), EntryURL);

            Console.WriteLine("Created Client w/ ID: " + this.url);
        }


        public void Submit(string inputPath, string outputPath, int splits, string className, string dllPath)
        {
            this.job = true;

            Console.WriteLine("Sumbit( " + inputPath + ",  " + outputPath + ",  " + splits + ",  " + className + ",  " + dllPath + ")");

            if (!Directory.Exists(outputPath))
            {
                Console.WriteLine("ERROR: Output folder does not exist!: " + outputPath);
                Console.WriteLine(ABORT_MESSAGE);
                return;
            }

            byte[] mapperCode = null;

            try
            {
                mapperCode = File.ReadAllBytes(dllPath);

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("ERROR: Input dll path doesn't exist");
                Console.WriteLine(ABORT_MESSAGE);
                return;
            }

            this.inputPath = inputPath;
            this.outputPath = outputPath;

            //Keeps the actual number of splits done by the splitFile method (may be less than specified)
            int actualSplits = 0;

            try
            {
                //Splits the file and creates a 'Splits' dictionary
                actualSplits = splitFile(splits);

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("ERROR: Input path doesn't exist");
                Console.WriteLine(ABORT_MESSAGE);
                return;
            }


            Console.WriteLine("Submiting...");

            //Submits the job to the known worker node
            this.localWorker.submit(actualSplits, mapperCode, className, this.url);
            Console.WriteLine("Submit Ended");

        }

        private int splitFile(int splits)
        {
            Console.WriteLine("[splitFile] Memory is " + GC.GetTotalMemory(false));

            // Gets the size of the input fily in bytes
            FileInfo f = new FileInfo(this.inputPath);
            int length = (int)f.Length;

            // Calculates the size of each split
            int chunkSize = (length / splits);

            int splitCount = 1;
            int fileIndex = 0;
            SplitInfo splitinfo;

            using (var input = new FileStream(this.inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 10))
            {

                int extraSize = 0;
                for (int i = 0; i < splits; i++)
                {

                    int byteStart = i * chunkSize + extraSize;

                    int byteEnd = (i + 1) * chunkSize;

                    if (byteEnd > length)
                        break;

                    input.Seek(byteEnd, SeekOrigin.Begin);


                    int extraByte = input.ReadByte();

                    while (extraByte != '\n' && extraByte != -1)
                    {
                        extraSize++;
                        extraByte = input.ReadByte();
                    }
                    byteEnd += extraSize;


                    splitinfo.pos = byteStart;
                    splitinfo.length = byteEnd - byteStart;
                    this.splitsDictionary.Add(i + 1, splitinfo);
                    splitCount = i;

                }
                input.Close();
            }


            return splitCount + 1;
        }

        // Return a split of the input file to a worker node
        public byte[] returnSplit(int splitNumber)
        {
            Console.WriteLine("returnSplit(" + splitNumber + ")");
            //MAD HAX - Isto n devia ser necessário...
            GC.Collect();
            GC.WaitForPendingFinalizers();

            byte[] result;
            using (BinaryReader b = new BinaryReader(File.OpenRead(this.inputPath)))
            {

                int pos = splitsDictionary[splitNumber].pos;
                int length = splitsDictionary[splitNumber].length;
                Console.WriteLine("   START: " + pos);
                Console.WriteLine("   SIZE: " + length);

                b.BaseStream.Seek(pos, SeekOrigin.Begin);


                result = b.ReadBytes(length);
                b.Close();
                b.Dispose();
            }


            return result;
        }

        // Obtains the results generated by the processing of a split from a worker node
        public void onSplitDone(string mapContent, int splitNumber)
        {
            Console.WriteLine("onSplitDone(" + splitNumber + ")");
            System.IO.File.WriteAllText(this.outputPath + splitNumber + ".out", mapContent);

        }

        //All splits processed and returned
        public void onJobDone()
        {
            Console.WriteLine("onJobDone()");
            splitsDictionary.Clear();
            this.job = false;
            Console.WriteLine("Job Done");
        }

        //Checks if the client is currently working on a job
        public bool hasJob()
        {
            Console.WriteLine("hasJob()");
            return this.job;
        }

    }
}
