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
using System.Net.Sockets;


namespace PuppetMaster
{

    //Delegate to handle creation of new workers
    public delegate void NewWorkerHandler(string urlNode);

    class PuppetMaster : MarshalByRefObject, IPuppetMaster
    {

        // Event
        public event NewWorkerHandler NewWorkerEvent;

        //PuppetMaster Variables
        private readonly TcpChannel channel = null;
        StreamReader scriptStreamReader = null;
        //public Dictionary<string, NodeData> nodeList = null;
        private List<NodeData> nodeList = new List<NodeData>();
        string url = string.Empty;

        const string BASEDIR = "../../../";

   
        public PuppetMaster(int puppetPort)
        {
            this.channel = new TcpChannel(puppetPort);
            this.url = "tcp://localhost:" + puppetPort + "/PM";
            //this.nodeList = new Dictionary<string, NodeData>();
            this.nodeList = new List<NodeData>();

            ChannelServices.RegisterChannel(this.channel, false);
            RemotingServices.Marshal(this, "PM", typeof(PuppetMaster));
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
                case "STATUS":
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
                startInfo.Arguments = id + " " + serviceUrl;
                process.StartInfo = startInfo;
                System.Threading.Thread.Sleep(500);
                process.Start();
                NewWorkerEvent(serviceUrl);

                //nodeList.Add(new NodeData("tcp://" + Util.LocalIPAddress() + ":" + serviceUrl + "/W"));
                nodeList.Add(new NodeData(serviceUrl));
            }
            else if (input.Length == 5)
            {

                string entryUrl = input[4];

                startInfo.Arguments = id + " " + serviceUrl + " " + entryUrl;
                process.StartInfo = startInfo;            
                System.Threading.Thread.Sleep(500);
                process.Start();
                NewWorkerEvent(serviceUrl);

                //nodeList.Add(new NodeData("tcp://" + Util.LocalIPAddress() + ":" + serviceUrl + "/W"));
                nodeList.Add(new NodeData(serviceUrl));

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

                string inputPath = BASEDIR + input[2];
                string outputPath = BASEDIR + input[3];
                int splits = Convert.ToInt32(input[4]);
                string className = input[5];
                string dllPath = BASEDIR + input[6];

                c.Submit(inputPath, outputPath, splits, className, dllPath);

                //IClient c = (IClient)p.CreateObjRef(typeof(IClient));
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR: " + e.ToString());
            }
        }

        private void processWait(string[] input)
        {
            // in seconds
            int waitTime = Convert.ToInt32(input[1]) * 1000;
            System.Threading.Thread.Sleep(waitTime);
        }

        private void processStatus(string[] input)
        {
            if (nodeList.Count > 0)
            {
                foreach (NodeData nodeData in nodeList)
                {
                    string url = nodeData.URL;
                    ICluster node = (ICluster)Activator.GetObject(typeof(ICluster), url);
                    try
                    {
                        node.status();
                        break;
                    }
                    catch (SocketException) { 
                        // this node doesnt exist anymore, let's try other one
                    }
                }
            }
            else
            {
                MessageBox.Show("ERROR: Couldn't contact cluster");
            }
        }

        private void processSloww(string[] input)
        {
            int  nodeID = Convert.ToInt32(input[1]);
            int time = Convert.ToInt32(input[2]);

            if (nodeList.Count > 0)
            {
                string url = nodeList[0].URL;
                ICluster node = (ICluster)Activator.GetObject(typeof(ICluster), url);
                node.slowW(nodeID, time);
            }
            else
            {
                MessageBox.Show("ERROR: Couldn't contact cluster");
            }
        }

        private void processFreezew(string[] input)
        {
            int nodeID = Convert.ToInt32(input[1]);

            if (nodeList.Count > 0)
            {
                string url = nodeList[0].URL;
                ICluster node = (ICluster)Activator.GetObject(typeof(ICluster), url);
                //node.freezeW(nodeID);
            }
            else
            {
                MessageBox.Show("ERROR: Couldn't contact cluster");
            }

        }

        private void processUnfreezew(string[] input)
        {
            int nodeID = Convert.ToInt32(input[1]);

            if (nodeList.Count > 0)
            {
                string url = nodeList[0].URL;
                ICluster node = (ICluster)Activator.GetObject(typeof(ICluster), url);
                //node.unFreezeW(nodeID);
            }
            else
            {
                MessageBox.Show("ERROR: Couldn't contact cluster");
            }

        }

        private void processFreezec(string[] input)
        {
            int nodeID = Convert.ToInt32(input[1]);

            if (nodeList.Count > 0)
            {
                string url = nodeList[0].URL;
                ICluster node = (ICluster)Activator.GetObject(typeof(ICluster), url);
                //node.freezeC(nodeID);
            }
            else
            {
                MessageBox.Show("ERROR: Couldn't contact cluster");
            }
        }

        private void processUnfreezec(string[] input)
        {
            int nodeID = Convert.ToInt32(input[1]);

            if (nodeList.Count > 0)
            {
                string url = nodeList[0].URL;
                ICluster node = (ICluster)Activator.GetObject(typeof(ICluster), url);
                //node.unFreezeC(nodeID);
            }
            else
            {
                MessageBox.Show("ERROR: Couldn't contact cluster");
            }
        }



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
