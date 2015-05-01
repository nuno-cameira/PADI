using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Padi.SharedModel;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

using ThreadPool;
using System.Reflection;
using System.Threading;
using System.IO;
using PADIMapNoReduce;
using System.Net.Sockets;

namespace Padi.Cluster
{



    // A delegate to handle the calls to a node in the cluster
    public delegate void ClusterHandler(INode node);


    public class Node : MarshalByRefObject, INode
    {

        //Node Variables
        private readonly TcpChannel channel = null;
        private readonly string url = null;
        private readonly int id;

        private CommunicationBehavior communicationBehavior = null;




        #region "Constructors"


        private int extractPortFromURL(string url)
        {
            int port = Convert.ToInt32(url.Split(':')[2].Split('/')[0]);
            return port;
        }


        /*
        * Constructs a singe node and registers it.
        */
        public Node(int id, string url, bool ensureSecurity)
        {
            Console.WriteLine("Creating Node...");

            int port = extractPortFromURL(url);
            this.channel = new TcpChannel(port);
            //this.url = "tcp://" + Util.LocalIPAddress() + ":" + port + "/W";
            this.url = url;

            this.id = id;

            this.communicationBehavior = new NormalCommunicationBehavior(this, id, url, ensureSecurity);

            ChannelServices.RegisterChannel(this.channel, ensureSecurity);
            RemotingServices.Marshal(this, "W", typeof(Node));

            Console.WriteLine("Created node w/ ID: " + id + " @ " + url);

        }


        /*
         * Constructor a node and adds it to the provided cluster.
         */
        public Node(int id, string url, bool ensureSecurity, string clusterURL)
            : this(id, url, ensureSecurity)
        {
            INode cluster = (INode)Activator.GetObject(typeof(INode), clusterURL);

            Console.WriteLine("Joining cluster @ " + clusterURL);

            this.communicationBehavior = new NormalCommunicationBehavior(this, id, url, ensureSecurity);

            //Attempt to join the cluster
            cluster.join(url);

        }


        #endregion



        #region "Properties
        public string URL { get { return communicationBehavior.url; } }

        public int ID { get { return communicationBehavior.id; } }

        public bool IsTracker { get { return communicationBehavior.trkUrl.Equals(this.url); } }

        //By busy we mean if it's working on a client split
        public bool IsBusy { get { return communicationBehavior.splitWork != -1; } }
        #endregion


        public void promote() { this.communicationBehavior.promote(); Console.WriteLine("DERP"); }
        public void setup(ClusterReport report) { this.communicationBehavior.setup(report); Console.WriteLine("DERP2"); }
        public bool doWork(int split, byte[] mapper, string className, string clientUrl) { this.communicationBehavior.doWork(split, mapper, className, clientUrl); Console.WriteLine("DERP3");  return false; }//"Submit" do Tracker
        public void printStatus() { this.communicationBehavior.printStatus(); Console.WriteLine("DERP4"); }

        //Cluster Events
        public void onTrackerChange(string peer) { this.communicationBehavior.onTrackerChange(peer); Console.WriteLine("DERP5"); }
        public void onClusterDecrease(string peer) { this.communicationBehavior.onClusterDecrease(peer); Console.WriteLine("DERP6"); }
        public void onClusterIncrease(string peer) { this.communicationBehavior.onClusterIncrease(peer); Console.WriteLine("DERP7"); }

        //Worker Events
        public void onSplitDone(string peer) { this.communicationBehavior.onSplitDone(peer); Console.WriteLine("DERP8"); }
        public void onSplitStart(string peer, int split, string clientUrl) { this.communicationBehavior.onSplitStart(peer, split, clientUrl); Console.WriteLine("DERP9"); }

        //Tracker events
        public void onJobDone(string clientUrl) { this.communicationBehavior.onJobDone(clientUrl); Console.WriteLine("DERP"); }
        public void onJobReceived(int splits, byte[] mapper, string classname, string clientUrl) { this.communicationBehavior.onJobReceived(splits, mapper, classname, clientUrl); Console.WriteLine("DERP10"); }
        public void disconect(string url) { this.communicationBehavior.disconect(url); Console.WriteLine("DERP11"); }





        #region "IWorker"

        /*
         * 
         */
        public void submit(int splits, byte[] mapper, string classname, string clientUrl)
        {
            communicationBehavior.submit(splits, mapper, classname, clientUrl);
        }

        private static EventWaitHandle halt =
     new EventWaitHandle(false, EventResetMode.AutoReset);



        #endregion


        /*
         * Region implementing the ICluster methods. These define the methods exposed to the Pupet master
         * 
         * All methods follow the same pattern:
         *  - Reveive message
         *  - Fowards message if its not tracker
         *  - Handles message if its tracker
         */
        #region "ICluster"




        /*
        * Adds a Node to the cluster.
        */
        public void join(string nodeUrl)
        {
            communicationBehavior.join(nodeUrl);
        }


        public void freezeW(int id)
        {
            communicationBehavior.freezeW(id);
        }


        public void freezeC(int id)
        {
            communicationBehavior.freezeC(id);
        }


        public void unFreezeW(int id)
        {
            communicationBehavior.unFreezeW(id);
        }


        public void unFreezeC(int id)
        {
            communicationBehavior.unFreezeC(id);
        }


        public void slowW(int id, int time)
        {
            communicationBehavior.slowW(id, time);
        }


        public void status()
        {
            communicationBehavior.status();
        }



        #endregion


        #region "Tracker"

        /*
        private bool assignTaskTo(INode node)
        {
            foreach (Job job in this.jobs)
            {

                if (job.hasSplits() && !node.IsBusy)
                {
                    node.doWork(job.assignSplit(node.URL), job.Mapper, job.ClassName, job.Client);
                    return true;
                }
            }
            return false;
        }
        */
        #endregion


    }
}
