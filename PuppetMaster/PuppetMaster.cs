using Padi.Cluster;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Windows.Forms;
using Padi.SharedModel;


namespace PuppetMaster
{

    //Delegate to handle creation of new workers
    public delegate void NewWorkerHandler(string urlNode);

    class PuppetMaster : MarshalByRefObject, IPuppetMaster
    {

        // Event
        public event NewWorkerHandler NewWorkerEvent;

        //PuppetMaster Variables
        const string EXTENSION = ".txt";
        private readonly TcpChannel channel = null;
        StreamReader scriptStreamReader = null;
        //public Dictionary<string, NodeData> nodeList = null;
        private List<NodeData> nodeList = new List<NodeData>();
        string url = string.Empty;

        // HARD CODED PUPPET PORT, CHANGE LATER!
        int puppetPort = 8999;


        public PuppetMaster()
        {
            this.channel = new TcpChannel(puppetPort);
            this.url = "tcp://localhost:" + puppetPort + "/PuppetMaster";
            //this.nodeList = new Dictionary<string, NodeData>();
            this.nodeList = new List<NodeData>();

            ChannelServices.RegisterChannel(this.channel, false);
            RemotingServices.Marshal(this, "PuppetMaster", typeof(PuppetMaster));
        }


        public void loadScript(string scriptName)
        {
            try
            {
                this.scriptStreamReader = File.OpenText(scriptName);
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }
        }


        /// <summary>
        /// Reads and executes all commands in scriptStreamReader until there's nothing more to read
        /// </summary>
        public void parser()
        {
            while (readLine() != null) ;
        }

        /// <summary>
        /// Reads and executes a single line in scriptStreamReader
        /// </summary>
        public string readLine()
        {
            string s = String.Empty;

            if ((s = scriptStreamReader.ReadLine()) != null)
            {
                executeCommand(s);
            }
            return s;
        }


        public void executeCommand(string s)
        {
            string[] input = s.Split(' ');

            switch (input[0])
            {
                case "WORKER":
                    createWorker(input);
                    break;
                case "SUBMIT":
                    processSubmit(input);
                    break;
                case "WAIT":
                    processWait(input);
                    break;
                case "STATUS:":
                    processStatus(input);
                    break;
                case "SLOWW":
                    processSloww(input);
                    break;
                case "FREEZEW":
                    processFreezew(input);
                    break;
                case "UNFREEZEW":
                    processUnfreezew(input);
                    break;
                case "FREEZEC":
                    processFreezec(input);
                    break;
                case "UNFREEZEC":
                    processUnfreezec(input);
                    break;
                default:
                    break;
            }
        }


        // TODO base code, erase when not needed
        public void parser2()
        {
            //If we want to do this listing all files in the root project directory
            /*string fullpath = Directory.GetCurrentDirectory();
            string path = Path.GetDirectoryName(fullpath);
            path = Path.GetDirectoryName(path);
            path = Path.GetDirectoryName(path);*/

            //maybe check if file exists 1st
            //StreamReader sr = File.OpenText("../../../" + scriptName + EXTENSION);
            string s = String.Empty;

            //Path.Combine(basePath, filePath);

            // TODO Any line in a PuppetMaster script starting with a ”%” sign should be ignored.

            /* TODO The GUI shall allow to load a script,
             * execute it step-by-step and without
             * interruptions
             */

            /*while ((s = sr.ReadLine()) != null)
            {
                string[] input = s.Split(' ');

                switch (input[0])
                {
                    case "WORKER":
                        createWorker(input);
                        break;
                    case "SUBMIT":
                        processSubmit(input);
                        break;
                    case "WAIT":
                        processWait(input);
                        break;
                    case "STATUS:":
                        processStatus(input);
                        break;
                    case "SLOWW":
                        processSloww(input);
                        break;
                    case "FREEZEW":
                        processFreezew(input);
                        break;
                    case "UNFREEZEW":
                        processUnfreezew(input);
                        break;
                    case "FREEZEC":
                        processFreezec(input);
                        break;
                    case "UNFREEZEC":
                        processUnfreezec(input);
                        break;
                    default:
                        break;
                }
                //do minimal amount of work here
            }*/
        }

        public void processWorker(string[] input)
        {
            string id = input[1];
            string serviceUrl = input[3];

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "Cluster.exe";

            if (input.Length == 4)
            {
                startInfo.Arguments = id + " " + serviceUrl +" +l "+this.url;
                process.StartInfo = startInfo;
                System.Threading.Thread.Sleep(500);
                process.Start();
                NewWorkerEvent(serviceUrl);
            }
            else if (input.Length == 5)
            {

                string entryUrl = input[4];

                startInfo.Arguments = id + " " + serviceUrl + " " + entryUrl;
                process.StartInfo = startInfo;            
                System.Threading.Thread.Sleep(500);
                process.Start();
                NewWorkerEvent(serviceUrl);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void createWorker(string[] input)
        {
            string url = input[2];

            IPuppetMaster master;
            if (this.url != url)
                master = (IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), url);
            else
                master = this;

            master.processWorker(input);
        }

        private void processSubmit(string[] input)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "Client.exe";
            startInfo.Arguments = input[1];
            Process p = Process.Start(startInfo);
            try
            {
                // TODO HARDCODED PORT, CHANGE LATER!
                string url = "tcp://" + Util.LocalIPAddress() + ":" + "10001" + "/Client";
                IClient c = (IClient)Activator.GetObject(typeof(IClient), url);

                string inputPath = input[2];
                string outputPath = input[3];
                int splits = Convert.ToInt32(input[4]);
                string className = input[5];
                string dllPath = input[6];

                c.Submit(inputPath, outputPath, splits, className, dllPath);

                //IClient c = (IClient)p.CreateObjRef(typeof(IClient));
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR: " + e.ToString());
            }


            MessageBox.Show("SUBMIT DONE");

            //throw new NotImplementedException();
        }

        private void processWait(string[] input)
        {
            // in seconds
            int waitTime = Convert.ToInt32(input[1]) * 1000;
            System.Threading.Thread.Sleep(waitTime);
        }

        private void processStatus(string[] input)
        {
            throw new NotImplementedException();
        }

        private void processSloww(string[] input)
        {
            throw new NotImplementedException();
        }

        private void processFreezew(string[] input)
        {
            string nodeID = input[1];

            if (nodeList.Count > 0)
            {
                string url = nodeList[0].URL;
                INode node = (INode)Activator.GetObject(typeof(INode), url);
                //node.freeze(nodeID);
            }
            else
            {
                MessageBox.Show("ERROR: Couldn't contact cluster");
            }

        }

        private void processUnfreezew(string[] input)
        {
            /*TODO do we need to save the state of the object 
             * in order to load it again with the same state?
             * or does it just come to life as a completly new object
             */

            throw new NotImplementedException();
        }





      


        private void processFreezec(string[] input)
        {
            throw new NotImplementedException();
        }

        private void processUnfreezec(string[] input)
        {
            throw new NotImplementedException();
        }




        #region "Cluster Events"
        public event JoinEventHandler JoinEvent;
        public event DisconectedEventHandler DisconectedEvent;
        public event TrackerChangeEventHandler TrackerChangeEvent;
        public event WorkStartEventHandler WorkStartEvent;
        public event WorkEndEventHandler WorkEndEvent;
        public event JobDoneEventHandler JobDoneEvent;
        public event NewJobEventHandler NewJobEvent;



        public void reportJoinEvent(string sender, string newNode)
        {
            if (JoinEvent!=null) JoinEvent(newNode);
        }

        public void reportDisconectionEvent(string sender, string oldNode)
        {
            if (DisconectedEvent != null) DisconectedEvent(oldNode);
        }

        public void reportTrackerChangeEvent(string sender, string newTracker)
        {
            if (TrackerChangeEvent != null) TrackerChangeEvent(newTracker);
        }

        public void reportWorkStartEvent(string sender, int split, string clientUrl)
        {
            if (WorkStartEvent != null) WorkStartEvent(split, clientUrl);
        }

        public void reportWorkEndEvent(string sender, int split, string clientUrl)
        {
            if (WorkEndEvent != null) WorkEndEvent(split, clientUrl);
        }

        public void reportJobDoneEvent(string sender, string clientUrl)
        {
            if (JobDoneEvent != null) JobDoneEvent(clientUrl);
        }

        public void reportNewJobEvent(string sender, int splits, byte[] mapper, string classname, string clientUrl)
        {
            if (NewJobEvent != null) NewJobEvent(splits, mapper, classname, clientUrl); 
        }
        #endregion
    }


    class NodeData
    {
        public string url;

        public NodeData(string url)
        {
            this.url = url;

        }

        public string URL
        {
            get { return this.url; }

        }

    }






    

}
