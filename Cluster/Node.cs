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

namespace Padi.Cluster
{

    //Delegate to handle the received messages
    public delegate void JoinEventHandler(string url);
    //Delegate to handle the received messages
    public delegate void DisconectedEventHandler(string url);
    // A delegate to handle the calls to a node in the cluster
    public delegate void ClusterHandler(INode node);


    public class Node : MarshalByRefObject, INode
    {
        //Node Variables
        private readonly TcpChannel channel = null;
        private readonly string url = null;
        private readonly int id;
        private bool isTracker;

        //Cluster status Variables
        private Dictionary<string, NodeStatus> cluster = null;
        private INode tracker = null;
        private ThrPool workThr = null;
        //Job Queue
        private List<Job> jobs; 


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
            this.cluster = new Dictionary<string, NodeStatus>();
            this.tracker = this;
            this.isTracker = true;
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

            ClusterReport report = cluster.join(this.URL);
            string trackerURL = report.Tracker;

            if (trackerURL != clusterURL)
            {
                this.tracker = (INode)Activator.GetObject(typeof(INode), trackerURL);
            }
            else
            {
                this.tracker = cluster;
            }
            Console.WriteLine("Joined cluster @ " + this.tracker.URL);
            this.isTracker = false;

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
            if (this.tracker != this)
            {
                return this.tracker.join(nodeUrl);
            }
            else
            {
                INode newPeer = (INode)Activator.GetObject(typeof(INode), nodeUrl);


                clusterAction((node) => { node.onClusterIncrease(nodeUrl); }, true);


                ClusterReport report = new ClusterReport();
                report.View = 1;//No use as for now
                report.Tracker = this.URL;
                report.Cluster = new List<string>(this.cluster.Keys);


                return report;
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
                INode node = (INode)Activator.GetObject(typeof(INode), lowestURL);
                node.promote();
            }
        }

        public void promote()
        {
            lock (this)
            {
                if (!this.isTracker)
                {
                    Console.WriteLine("What?\n Worker is evolving!");
                    Dictionary<string, NodeStatus> newCluster = new Dictionary<string, NodeStatus>();
                    foreach (KeyValuePair<string, NodeStatus> entry in cluster)
                    {

                        INode node = (INode)Activator.GetObject(typeof(INode), entry.Key);

                        newCluster.Add(entry.Key, new NodeStatus(node, entry.Value.isWorking));
                    }

                    this.cluster = newCluster;
                    this.isTracker = true;
                    this.tracker = this;
                    clusterAction((node) => { node.onTrackerChange(this.URL); }, true);
                }

            }
        }

        private void clusterAction(ClusterHandler onSucess, bool warnDisconections)
        {
            clusterAction(onSucess, warnDisconections, true);
        }

        /// <summary>
        /// Calls the delegate function to every Node in the cluster
        /// </summary>
        /// <param name="onSucess">Delegate function to call for each Node in the cluster</param>
        /// <param name="warnDisconections">Warn the nodes of eventual disconections</param>
        private void clusterAction(ClusterHandler onSucess, bool warnDisconections, bool incTrack)
        {
            List<string> disconected = new List<string>();


            foreach (KeyValuePair<string, NodeStatus> entry in cluster)
            {



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
            if (this.tracker != this)
            {
                this.tracker.disconect(peer);
            }
            else
            {
                lock (cluster)
                {
                    if (cluster.ContainsKey(peer))
                    {
                        cluster.Remove(peer);
                        clusterAction((node) =>
                        {
                            node.onClusterDecrease(peer);
                        }, true);
                    }

                }
            }
        }


        #endregion


        #region "WORKER"
        public void submit(int splits, byte[] mapper, string clientUrl)
        {
            if (this.isTracker)
            {
                //Make a new job
                Job job = new Job(splits, mapper, clientUrl);
                jobs.Add(job);

                clusterAction((node) =>
                {
                    //Assign specific job to each node
                    if (job.hasSplits())
                        node.doWork(job.assignSplit(node.URL), mapper, clientUrl);

                    //Inform all nodes about new job
                    node.onJobReceived(splits, mapper, clientUrl);

                }, true, false);
            }
            else
            {
                this.tracker.submit(splits, mapper, clientUrl);
                return;
            }
        }


        public bool doWork(int split, byte[] mapper, string clientUrl)
        {
            Console.WriteLine("Node::doWork(" + split + " , mapper , " + clientUrl + ")");
            return true;
        }


        #endregion

        #region "Events"
        public void onClusterIncrease(string peer)
        {
            INode node = null;
            if (this.isTracker) { 

                node = (INode)Activator.GetObject(typeof(INode), peer);
      
                foreach (Job job in this.jobs) {
                    
                    if (job.hasSplits()) {
               
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
            if (!this.isTracker) { this.tracker = (INode)Activator.GetObject(typeof(INode), p); }
        }


        public void onSplitDone(string peer) { }
        public void onSplitStart(string peer, int split, string clientUrl) { }


        public void onJobDone(string clientUrl) { }
        public void onJobReceived(int splits, byte[] mapper, string clientUrl) { }


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
