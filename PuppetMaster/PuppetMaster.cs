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


namespace PuppetMaster
{
    class PuppetMaster : MarshalByRefObject, IPuppetMaster
    {
        const string EXTENSION = ".txt";
        private readonly TcpChannel channel = null;
        string scriptName = String.Empty;
        private Dictionary<string, NodeStatus> nodeList = null;
        //private List<Node> nodeList = new List<Node>();
        string URL = string.Empty;

        // HARD CODED PUPPET PORT, CHANGE LATER!
        int puppetPort = 8999;


        public PuppetMaster()
        {
            this.channel = new TcpChannel(puppetPort);
            this.URL = "tcp://localhost:" + puppetPort + "/PuppetMaster";
            this.nodeList = new Dictionary<string, NodeStatus>();

            ChannelServices.RegisterChannel(this.channel, false);
            RemotingServices.Marshal(this, "PuppetMaster", typeof(PuppetMaster));
        }


        public void loadScript(string scriptName)
        {
            this.scriptName = scriptName;
        }


        public void parser()
        {
            //If we want to do this listing all files in the root project directory
            /*string fullpath = Directory.GetCurrentDirectory();
            string path = Path.GetDirectoryName(fullpath);
            path = Path.GetDirectoryName(path);
            path = Path.GetDirectoryName(path);*/

            //maybe check if file exists 1st
            StreamReader sr = File.OpenText("../../../" + scriptName + EXTENSION);
            string s = String.Empty;

            //Path.Combine(basePath, filePath);

            // TODO Any line in a PuppetMaster script starting with a ”%” sign should be ignored.

            /* TODO The GUI shall allow to load a script,
             * execute it step-by-step and without
             * interruptions
             */

            while ((s = sr.ReadLine()) != null)
            {
                string[] input = s.Split(' ');

                string ferf = input[0];

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
            }
        }

        public void processWorker(string[] input)
        {
            if (input.Length == 4)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "Cluster.exe";
                startInfo.Arguments = input[1] + " " + input[3];
                Process.Start(startInfo);
            }
            else if (input.Length == 5)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "Cluster.exe";
                startInfo.Arguments = input[1] + " " + input[3] + " " + input[4];
                Process.Start(startInfo);
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
            if (this.URL != url)
                master = (IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), url);
            else
                master = this;

            master.processWorker(input);
        }

        private void processSubmit(string[] input)
        {
            throw new NotImplementedException();
        }

        private void processWait(string[] input)
        {
            throw new NotImplementedException();
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
            string url = input[1];

            // TODO see if it's already freezed

            //disconnects node with url
            NodeStatus nodeS = nodeList[url];
            nodeS.node.disconect(url);
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





    }
}
