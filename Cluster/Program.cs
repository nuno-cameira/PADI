using Padi.SharedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMaster;
namespace Padi.Cluster
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Missing arguments");
                return;
            }

            Node node;





            if (args.Length == 2 || args.Length == 4)
            {
                node = new Node(int.Parse(args[0]), int.Parse(args[1]), false);
            }
            else
            {
                node = new Node(int.Parse(args[0]), int.Parse(args[1]), false, args[2]);
            }


            string s;
            for (int i = 0; i < args.Length; i++)
            {
                s = args[i];

                if (s.Equals("+l") && i + 1 < args.Length)
                {
                    addPuppetListener(node, args[i + 1]);
                    break;

                }
            }
            node.JoinEvent += onJoinEvent;
            node.DisconectedEvent += onDisconectedEvent;

            Console.WriteLine("Server Ready!");
            Console.WriteLine("Type \"kill\" to close the server.");
            string input;

            input = System.Console.ReadLine();
            int splits = Int16.Parse(input);
            node.submit(splits, null, "", args[1]);

            int t = 2;
            while (t != 0)
            {
                input = System.Console.ReadLine();
                node.freezeW(2);

                input = System.Console.ReadLine();
                node.unFreezeW(2);
                t--;

            }



            node = null;
            Console.WriteLine("Server killed.");
            Environment.Exit(0);
        }




        private static void addPuppetListener(Node node, string puppet)
        {
            Console.WriteLine("addPuppetListener "+puppet);

            IPuppetMaster pMaster = (IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), puppet);

            string sender = node.URL;
            node.JoinEvent += (url) => { pMaster.reportJoinEvent(sender, url); };
            node.DisconectedEvent += (url) => { pMaster.reportDisconectionEvent(sender, url); };
            node.TrackerChangeEvent += (url) => { pMaster.reportTrackerChangeEvent(sender, url); };
            node.WorkStartEvent += (peer, split, clientUrl) => { pMaster.reportWorkStartEvent(sender, peer, split, clientUrl); };
            node.WorkEndEvent += (peer) => { pMaster.reportWorkEndEvent(sender, peer); };
            node.JobDoneEvent += (url) => { pMaster.reportJobDoneEvent(sender, url); };
            node.NewJobEvent += (splits, mapper, classname, clientUrl) => { pMaster.reportNewJobEvent(sender, splits, mapper, classname, clientUrl); };
        }




        private static void onDisconectedEvent(string url)
        {
            Console.WriteLine("onDisconectedEvent: " + url);
        }

        private static void onMessageEvent(string sender, string msg)
        {
            Console.WriteLine("onMessageEvent: " + sender + " " + msg);
        }

        private static void onJoinEvent(string url)
        {
            Console.WriteLine("onJoinEvent: " + url);
        }
    }
}
