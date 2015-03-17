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
        private Dictionary<string, INode> cluster = null;
        private INode tracker = null;

        //Event
        public event JoinEventHandler JoinEvent;
        public event DisconectedEventHandler DisconectedEvent;



        #region "Properties"

        public string URL
        {
            get { return this.url; }

        }


        #endregion



        #region "Constructors"

        /// <summary>
        /// Constructs a singe node and registers it.
        /// </summary>
        public Node(int port, bool ensureSecurity)
        {
            this.channel = new TcpChannel(port);
            this.url = "tcp://localhost:" + port + "/Node";
            this.cluster = new Dictionary<string, INode>();
            this.tracker = this;

            Console.WriteLine("RegisterChannel " + port);
            ChannelServices.RegisterChannel(this.channel, ensureSecurity);
            RemotingServices.Marshal(this, "Node", typeof(INode));

        }


        /// <summary>
        /// Constructor a node and adds it to the providade cluster.
        /// </summary>
        public Node(int port, bool ensureSecurity, string clusterURL)
            : this(port, ensureSecurity)
        {
            INode cluster = (INode)Activator.GetObject(typeof(INode), clusterURL);

            string trackerURL = cluster.join(this.URL);

            if (trackerURL != clusterURL)
            {
                this.tracker = (INode)Activator.GetObject(typeof(INode), trackerURL);
            }
            else
            {
                this.tracker = cluster;
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


                foreach (INode node in cluster.Values)
                {
                    node.onClusterIncrease(nodeUrl);
                }

                cluster.Add(nodeUrl, newPeer);
                onClusterIncrease(nodeUrl);
                return this.URL;
            }
        }


        public void sendMessage(string sender, string msg)
        {
            if (this.tracker != this)
            {
                this.tracker.sendMessage(sender, msg);
            }
            else
            {
                clusterAction((node) =>
                {
                    if (node.URL != sender)
                        node.onClusterMessage(msg);
                }, true, true);
            }
        }



        public delegate void ClusterHandler(INode node);
        private void clusterAction(ClusterHandler onSucess, bool warnDisconections, bool recursive)
        {
            List<string> disconected = new List<string>();

            lock (cluster)
            {
                string tryKey = "";
                try
                {
                    foreach (KeyValuePair<string, INode> entry in cluster)
                    {
                        tryKey = entry.Key;
                        onSucess(entry.Value);
                    }
                }
                catch
                {
                    disconected.Add(tryKey);
                }
                onSucess(this);
            }

            if (disconected.Count != 0)
            {
                lock (cluster)
                {
                    foreach (string badNode in disconected)
                    {
                        cluster.Remove(badNode);
                    }
                }

                clusterAction((node) =>
                {
                    foreach (string badNode in disconected)
                    {
                        node.onClusterDecrease(badNode);
                    }

                }, true, true);
            }
        }







        public void onClusterIncrease(string peer) { JoinEvent(peer); }
        public void onClusterDecrease(string peer) { DisconectedEvent(peer); }
        public void onClusterMessage(string msg) { Console.WriteLine(msg); }
    }
}
