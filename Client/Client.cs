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

namespace Client
{
    public class Client : MarshalByRefObject, IClient
    {
        private static readonly string ABORT_MESSAGE = "Aborting submition...";

        //Local worker node used to submit jobs
        private IWorker localWorker = null;

        private readonly TcpChannel channel = null;
        private readonly int clientPort = 10001;
        private readonly string url = null;

        private string inputPath = null;
        private string outputPath = null;

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



        public Client(string EntryURL)
        {
            Console.WriteLine("Creating the Client...");

            this.channel = new TcpChannel(clientPort);
            this.url = "tcp://" + Util.LocalIPAddress() + ":" + clientPort + "/Client";

            ChannelServices.RegisterChannel(this.channel, false);
            RemotingServices.Marshal(this, "Client", typeof(Client));


            this.localWorker = (IWorker)Activator.GetObject(typeof(IWorker), EntryURL);
        }


        public void Submit(string inputPath, string outputPath, int splits, string className, string dllPath)
        {
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
            byte[] file = null;


            file = File.ReadAllBytes(this.inputPath);
            int chunkSize = file.Length / splits;

            byte[] buffer = new byte[chunkSize];
            List<byte> extraBuffer = new List<byte>();

            int splitIndex = 1;

            int fileIndex = 0;

            using (Stream input = File.OpenRead(this.inputPath))
            {              
                while (input.Position < input.Length)
                {

                    int bytesRead = input.Read(buffer, 0, chunkSize);

                    byte extraByte = buffer[chunkSize - 1];
                    while (extraByte != '\n')
                    {
                        int flag = input.ReadByte();
                        if (flag == -1)
                            break;
                        extraByte = (byte)flag;
                        extraBuffer.Add(extraByte);
                    }

                    byte[] extraBufferArray = extraBuffer.ToArray();

                    SplitInfo splitinfo = new SplitInfo(fileIndex, buffer.Length + extraBufferArray.Length);

                    this.splitsDictionary.Add(splitIndex, splitinfo);


                    Array.Clear(buffer, 0, chunkSize);
                    extraBuffer.Clear();

                    fileIndex += bytesRead;
                    splitIndex++;
                }
                return splitIndex - 1;
            }
        }

        // Return a split of the input file to a worker node
        public byte[] returnSplit(int splitNumber)
        {
            using (BinaryReader b = new BinaryReader(File.OpenRead(this.inputPath)))
            {
                int pos = splitsDictionary[splitNumber].pos;
                int length = splitsDictionary[splitNumber].length;

                b.BaseStream.Seek(pos, SeekOrigin.Begin);

                return b.ReadBytes(length);
            }
        }

        // Obtains the results generated by the processing of a split from a worker node
        public void onSplitDone(string mapContent, int splitNumber)
        {

            System.IO.File.WriteAllText(this.outputPath + splitNumber + ".out", mapContent);

        }

        //All splits processed and returned
        public void onJobDone()
        {
            splitsDictionary.Clear();
            Console.WriteLine("Job Done");
        }

    }
}
