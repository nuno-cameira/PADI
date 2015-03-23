using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Padi.SharedModel;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace Padi.Cluster
{
    //Delegate to handle the received messages
    public delegate void JoinEventHandler(string url);
    //Delegate to handle the received messages
    public delegate void DisconectedEventHandler(string url);


    public class Node : MarshalByRefObject, INode
    {
        private readonly TcpChannel channel = null;
        private readonly string url = null;
        private readonly int id;
        private Dictionary<string, INode> cluster = null;
        private INode tracker = null;
        private ThrPool workThr = null;
        private bool isTracker;

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
            this.url = "tcp://localhost:" + port + "/Node";
            this.cluster = new Dictionary<string, INode>();
            this.tracker = this;
            this.isTracker = true;
            this.workThr = new ThrPool(10, 50);
            this.id = id;

            
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
            
            Console.WriteLine("Joining cluster @ "+cluster.URL);
            string trackerURL = cluster.join(this.URL);

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

            List<string> clus = this.tracker.getCluster();

            foreach (string s in clus)
            
            {
                if (s != this.URL) { onClusterIncrease(s); }
            }
        }

 
        #endregion

        /// <summary>
        /// Receives a Node's URL and adds it to the cluster.
        /// </summary>
        public string join(string nodeUrl)
        {
            if (this.tracker != this)
            {
                return this.tracker.join(nodeUrl);
            }
            else
            {
                INode newPeer = (INode)Activator.GetObject(typeof(INode), nodeUrl);


                clusterAction((node) => { node.onClusterIncrease(nodeUrl); }, true);

                return this.URL;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        public void sendMessage(string sender, string msg)
        {
            if (this.tracker != this)
            {
                try
                {
                    this.tracker.sendMessage(sender, msg);
                }
                catch
                {
                    tryPromote();
                }

            }
            else
            {
                clusterAction((node) =>
                {
                    if (node.URL != sender)
                        node.onClusterMessage(msg);
                }, true);
            }
        }

        public void tryPromote()
        {
            string lowestURL = this.URL;
            foreach (KeyValuePair<string, INode> entry in cluster)
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
                    Dictionary<string, INode> newCluster = new Dictionary<string, INode>();
                    foreach (KeyValuePair<string, INode> entry in cluster)
                    {

                        INode node = (INode)Activator.GetObject(typeof(INode), entry.Key);
                        newCluster.Add(entry.Key, node);
                    }

                    this.cluster = newCluster;
                    this.isTracker = true;
                    this.tracker = this;
                    clusterAction((node) => { node.onTrackerChange(this.URL); }, true);
                }

            }
        }




        /// <summary>
        /// A delegate to handle the calls to a node in the cluster
        /// </summary>
        /// <param name="node"></param>
        public delegate void ClusterHandler(INode node);



        /// <summary>
        /// Calls the delegate function to every Node in the cluster
        /// </summary>
        /// <param name="onSucess">Delegate function to call for each Node in the cluster</param>
        /// <param name="warnDisconections">Warn the nodes of eventual disconections</param>
        private void clusterAction(ClusterHandler onSucess, bool warnDisconections)
        {
            List<string> disconected = new List<string>();


            foreach (KeyValuePair<string, INode> entry in cluster)
            {

                this.workThr.AssyncInvoke(() =>
                {
                    try
                    {
                        onSucess(entry.Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        disconect(entry.Key);
                    }
                });
            }
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

        public List<string> getCluster()
        {
            if (this.isTracker)
            {
                List<string> clusterURLs = new List<string>();

                foreach (KeyValuePair<string, INode> entry in cluster)
                {

                    clusterURLs.Add(entry.Key);
                }
                return clusterURLs;
            }
            else
            {
                return this.tracker.getCluster();
            }

        }


        public void onClusterIncrease(string peer)
        {
            INode node = null;
            if (this.isTracker) { node = (INode)Activator.GetObject(typeof(INode), peer); }

            lock (cluster) {   cluster.Add(peer, node); }
            if (JoinEvent != null) {JoinEvent(peer); }
        }
        public void onClusterDecrease(string peer)
        {
            lock (cluster) { cluster.Remove(peer); }
            if (DisconectedEvent != null) DisconectedEvent(peer);
        }

        public void onClusterMessage(string msg)
        {
            Console.WriteLine(msg);
        }


        public void onTrackerChange(string p)
        {
            Console.WriteLine("onTrackerChange");
            if (!this.isTracker) { this.tracker = (INode)Activator.GetObject(typeof(INode), p); }
        }
    }
}
