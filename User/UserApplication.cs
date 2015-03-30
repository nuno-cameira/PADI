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

        //É preciso ver se isto é mesmo suposto ser assim...
        private string clientUrl = null;
        private readonly int clientPort = 8300;


        public void Init(string EntryURL)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "Client.exe";
            startInfo.Arguments = EntryURL;
            Process.Start(startInfo);

            this.clientUrl = "tcp://" + Util.LocalIPAddress() + ":" + clientPort + "/Client";           

        }

        public void Submit(string inputPath, string outputPath, int splits, string className, string dllPath)
        {
            this.localClient = (IClient)Activator.GetObject(typeof(IClient), clientUrl);

            this.localClient.Submit(inputPath, outputPath, splits, className, dllPath);
        }

    }
}
