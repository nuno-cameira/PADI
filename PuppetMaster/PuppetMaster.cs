using Padi.Cluster;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PuppetMaster
{
    class PuppetMaster
    {
        const string EXTENSION = ".txt";
        string scriptName = String.Empty;
        private List<Node> nodeList = new List<Node>();

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
            while ((s = sr.ReadLine()) != null)
            {
                string[] input = s.Split(' ');

                string ferf = input[0];

                switch (input[0])
                {
                    case "WORKER":
                        processWorker(input);
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
                        processFreezen(input);
                        break;
                    case "UNFREEZEW":
                        processUnfreezen(input);
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

        private void processWorker(string[] input)
        {
            Node node = null;
            if (input.Length == 4)
            {
                node = new Node(Convert.ToInt32(input[3]), false);
            }
            else if (input.Length == 5)
            {
                node = new Node(Convert.ToInt32(input[3]), false, input[4]);
            }
            else
            {
                throw new NotImplementedException();
            }
            nodeList.Add(node);
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

        private void processFreezen(string[] input)
        {
            throw new NotImplementedException();
        }

        private void processUnfreezen(string[] input)
        {
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
