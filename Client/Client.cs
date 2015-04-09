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

        private string outputPath = null;
        private Dictionary<int, byte[]> splitsDictionary = new Dictionary<int, byte[]>();



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

            //Keeps the actual number of splits done by the splitFile method (may be less than specified)
            int actualSplits = 0;

            try
            {
                //Splits the file and creates a 'Splits' dictionary
                actualSplits = splitFile(inputPath, splits);

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("ERROR: Input path doesn't exist");
                Console.WriteLine(ABORT_MESSAGE);
                return;
            }


            Console.WriteLine("Submiting...");
            /*DEBUG CODE 
            foreach(var element in this.splitsDictionary){
                Console.WriteLine("KEY: " + element.Key);
                Console.WriteLine("VALUE: " + System.Text.Encoding.Default.GetString(returnSplit(element.Key)));
            }
            END DEBUG CODE*/

            this.outputPath = outputPath;

            //Submits the job to the known worker node
            this.localWorker.submit(actualSplits, mapperCode, className, this.url);
            Console.WriteLine("Submit Ended");

        }

        private int splitFile(string inputPath, int splits)
        {
            byte[] file = null;


            file = File.ReadAllBytes(inputPath);
            int chunkSize = file.Length / splits;
            byte[] buffer = new byte[chunkSize];
            List<byte> extraBuffer = new List<byte>();

            using (Stream input = File.OpenRead(inputPath))
            {
                int index = 1;
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

                    byte[] concat = new byte[buffer.Length + extraBufferArray.Length];
                    System.Buffer.BlockCopy(buffer, 0, concat, 0, buffer.Length);
                    System.Buffer.BlockCopy(extraBufferArray, 0, concat, buffer.Length, extraBufferArray.Length);
                    this.splitsDictionary.Add(index, concat);


                    Array.Clear(buffer, 0, chunkSize);
                    extraBuffer.Clear();

                    index++;
                }
                return index - 1;
            }
        }

        // Return a split of the input file to a worker node
        public byte[] returnSplit(int splitNumber)
        {
            return splitsDictionary[splitNumber];
        }

        // Obtains the results generated by the processing of a split from a worker node
        public void onSplitDone(string mapContent, int splitNumber)
        {

            System.IO.File.WriteAllText(this.outputPath + splitNumber + ".out", mapContent);

        }

        //All splits processed and returned
        public void onJobDone()
        {
            Console.WriteLine("Job Done!");
        }

    }
}
