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

namespace Padi.Cluster
{

    //Delegate to handle the received messages
    public delegate void JoinEventHandler(string url);
    //Delegate to handle the received messages
    public delegate void DisconectedEventHandler(string url);
    // A delegate to handle the calls to a node in the cluster
    public delegate object ClusterHandler(INode node);


    public class Node : MarshalByRefObject, INode
    {
        //Node Variables
        private readonly TcpChannel channel = null;
        private readonly string url = null;
        private readonly int id;

        private ThrPool workThr = null;
        private List<Job> jobs; //job queue

        //Cluster status Variables
        private Dictionary<string, NodeStatus> cluster = null;
        private INode tracker = null;
        private string trkUrl = "";

        //Event
        public event JoinEventHandler JoinEvent;
        public event DisconectedEventHandler DisconectedEvent;




        #region "Properties
        public string URL
        {
            get { return this.url; }

        }

        public int ID
        {
            get { return this.id; }

        }

        public bool IsTracker { get { return this.trkUrl.Equals(this.url); } }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructs a singe node and registers it.
        /// </summary>
        public Node(int id, int port, bool ensureSecurity)
        {
            Console.WriteLine("Creating Node...");

            this.channel = new TcpChannel(port);
            this.url = "tcp://" + Util.LocalIPAddress() + ":" + port + "/Node";
            this.trkUrl = this.url;
            this.cluster = new Dictionary<string, NodeStatus>();
            this.tracker = this;

            this.workThr = new ThrPool(10, 50);
            this.id = id;
            this.jobs = new List<Job>();



            ChannelServices.RegisterChannel(this.channel, ensureSecurity);
            RemotingServices.Marshal(this, "Node", typeof(Node));

            Console.WriteLine("Created node w/ ID: " + id + " @ " + this.URL);

        }


        /// <summary>
        /// Constructor a node and adds it to the providade cluster.
        /// </summary>
        public Node(int id, int port, bool ensureSecurity, string clusterURL)
            : this(id, port, ensureSecurity)
        {
            INode cluster = (INode)Activator.GetObject(typeof(INode), clusterURL);

            Console.WriteLine("Joining cluster @ " + clusterURL);

            ClusterReport report = null;

            //Attempt to join the cluster
            int attempts = 3;
            while (attempts != 0)
            {
                attempts--;
                report = cluster.join(this.URL);
                if (report != null)
                    break;

                Console.WriteLine("Retrying...");
            }

            //Check for valid cluster's answer 
            if (report == null)
            {
                Console.WriteLine("Failed to joining cluster @ " + clusterURL);
                return;
            }

            string trackerURL = report.Tracker;

            if (trackerURL != clusterURL)
            {
                this.tracker = (INode)Activator.GetObject(typeof(INode), trackerURL);
                this.trkUrl = trackerURL;
            }
            else
            {
                this.tracker = cluster;
                this.trkUrl = clusterURL;
            }

            Console.WriteLine("Joined cluster @ " + this.trkUrl);


            List<string> clus = new List<string>(report.Cluster);

            foreach (string s in clus)
            {
                if (s != this.URL) { onClusterIncrease(s); }
            }
        }


        #endregion


        #region "Cluster Actions"
        /// <summary>
        /// Receives a Node's URL and adds it to the cluster.
        /// </summary>
        public ClusterReport join(string nodeUrl)
        {


            if (this.IsTracker)
            {

                INode newPeer = (INode)Activator.GetObject(typeof(INode), nodeUrl);


                clusterAction((node) => { node.onClusterIncrease(nodeUrl); return null; });


                ClusterReport report = new ClusterReport();
                report.View = 1;//No use as for now
                report.Tracker = this.URL;
                report.Cluster = new List<string>(this.cluster.Keys);


                return report;
            }
            else
            {

                return (ClusterReport)nodeAction((trk) => { return trk.join(nodeUrl); }, this.trkUrl);
            }
        }


        private void tryPromote()
        {
            string lowestURL = this.URL;
            foreach (KeyValuePair<string, NodeStatus> entry in cluster)
            {
                if (entry.Key.GetHashCode() < this.URL.GetHashCode())
                {
                    lowestURL = entry.Key;
                    break;
                }
            }


            if (lowestURL.Equals(this.URL))
            {
                promote();
            }
            else
            {

                nodeAction((node) => { node.promote(); return null; }, lowestURL);
            }
        }

        public void promote()
        {
            lock (this)
            {
                if (!this.IsTracker)
                {
                    Console.WriteLine("What?\n Worker is evolving!");
                    Dictionary<string, NodeStatus> newCluster = new Dictionary<string, NodeStatus>();
                    foreach (KeyValuePair<string, NodeStatus> entry in cluster)
                    {

                        INode node = (INode)Activator.GetObject(typeof(INode), entry.Key);

                        newCluster.Add(entry.Key, new NodeStatus(node, entry.Value.isWorking));
                    }

                    this.cluster = newCluster;

                    this.tracker = this;
                    this.trkUrl = this.url;
                    clusterAction((node) => { node.onTrackerChange(this.URL); return null; });
                }

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSucess"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private object nodeAction(ClusterHandler onSucess, string url)
        {


            object sucess = null;


            INode node = null;
            if (url == this.trkUrl)
                node = this.tracker;
            else
                node = cluster[url].node;

            //Checks if proxy exists
            if (node == null)
                node = (INode)Activator.GetObject(typeof(INode), url);


            try
            {
                sucess = onSucess(node);
            }
            catch //remote server does not exist
            {
                if (url == this.trkUrl)
                {
                    tryPromote();
                }
                else
                {
                    nodeAction((trk) => { trk.disconect(url); return null; }, this.trkUrl);
                }
            }


            return sucess;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="onSucess"></param>
        /// <param name="warnDisconections"></param>
        private void clusterAction(ClusterHandler onSucess)
        {
            clusterAction(onSucess, true, false);
        }

        /// <summary>
        /// Calls the delegate function to every Node in the cluster
        /// </summary>
        /// <param name="onSucess">Delegate function to call for each Node in the cluster</param>
        /// <param name="warnDisconections">Warn the nodes of eventual disconections</param>
        private void clusterAction(ClusterHandler onSucess, bool incTrack, bool freeOnly)
        {
            List<string> disconected = new List<string>();


            foreach (KeyValuePair<string, NodeStatus> entry in cluster)
            {
                if (freeOnly && entry.Value.isWorking)
                    continue;


                this.workThr.AssyncInvoke(() =>
                {
                    try
                    {
                        onSucess(entry.Value.node);
                    }
                    catch
                    {

                        disconect(entry.Key);
                    }
                });
            }
            if (incTrack)
                onSucess(this);
        }



        public void disconect(string peer)
        {
            if (this.IsTracker)
            {
                lock (cluster)
                {
                    if (cluster.ContainsKey(peer))
                    {
                        cluster.Remove(peer);
                        clusterAction((node) =>
                        {
                            node.onClusterDecrease(peer);
                            return null;
                        });
                    }

                }

            }
            else
            {
                nodeAction((trk) => { trk.disconect(peer); return null; }, this.trkUrl);

            }
        }


        #endregion


        #region "Worker"
        public void submit(int splits, byte[] mapper, string clientUrl)
        {
            if (this.IsTracker)
            {

                onJobReceived(splits, mapper, clientUrl);

                clusterAction((node) =>
                {
                    Job job = jobs[0];
                    if (job != null)
                    {
                        //Assign specific job to each node
                        if (job.hasSplits())
                            node.doWork(job.assignSplit(node.URL), mapper, clientUrl);

                        //Inform all nodes about new job
                        node.onJobReceived(splits, mapper, clientUrl);
                    }

                    return null;
                }, false, true);
            }
            else
            {
                nodeAction((trk) => { trk.submit(splits, mapper, clientUrl); return null; }, this.trkUrl);
                return;
            }
        }


        public bool doWork(int split, byte[] mapper, string clientUrl)
        {
            Console.WriteLine("Node::doWork(" + split + " , mapper , " + clientUrl + ")");
            return true;
        }


        private IMapper loadMapper(byte[] code, string className)
        {
            Assembly assembly = Assembly.Load(code);

            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + className))
                    {
                        // create an instance of the object
                        return (IMapper)Activator.CreateInstance(type);
                    }
                }
            }

            throw (new System.Exception("could not invoke method"));
        }


        #endregion

        #region "Events"
        public void onClusterIncrease(string peer)
        {
            INode node = null;
            if (this.IsTracker)
            {

                node = (INode)Activator.GetObject(typeof(INode), peer);

                foreach (Job job in this.jobs)
                {

                    if (job.hasSplits())
                    {

                        node.doWork(job.assignSplit(peer), job.Mapper, job.Client);
                        break;
                    }
                }
            }

            lock (cluster) { if (!cluster.ContainsKey(peer)) { cluster.Add(peer, new NodeStatus(node)); } }
            if (JoinEvent != null) { JoinEvent(peer); }
        }
        public void onClusterDecrease(string peer)
        {
            lock (cluster) { cluster.Remove(peer); }
            if (DisconectedEvent != null) DisconectedEvent(peer);
        }


        public void onTrackerChange(string p)
        {
            Console.WriteLine("onTrackerChange");
            if (!this.IsTracker) { this.tracker = (INode)Activator.GetObject(typeof(INode), p); }
        }


        public void onSplitDone(string peer) { }
        public void onSplitStart(string peer, int split, string clientUrl) { }


        public void onJobDone(string clientUrl) { }
        public void onJobReceived(int splits, byte[] mapper, string clientUrl)
        {
            Console.WriteLine("onJobReceived");
            //Make a new job
            Job job = new Job(splits, mapper, clientUrl);
            jobs.Add(job);
        }


    }

        #endregion




    class NodeStatus
    {
        public INode node;
        public bool isWorking;
        //public  TIMESPAMP

        public NodeStatus(INode node)
        {
            this.node = node;
            this.isWorking = false;
        }

        public NodeStatus(INode node, bool isWorking)
        {
            this.node = node;
            this.isWorking = isWorking;
        }
    }


}
