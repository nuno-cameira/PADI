using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Padi.SharedModel;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;


namespace Padi.User
{
    public class UserApplication
    {
        private IClient localClient = null;
        private readonly int clientPort = 10001;


        public void Init(string EntryURL)
        {
            //Start the Client process
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "Client.exe";
            startInfo.Arguments = EntryURL;
            Process.Start(startInfo);

            
            this.localClient = (IClient)Activator.GetObject(typeof(IClient), "tcp://" + Util.LocalIPAddress() + ":" + clientPort + "/Client");

        }

        public void Submit(string inputPath, string outputPath, int splits, string className, string dllPath)
        {    
     
            this.localClient.Submit(inputPath, outputPath, splits, className, dllPath);

        }

    }
}
