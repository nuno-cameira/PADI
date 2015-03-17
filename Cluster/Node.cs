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
        private ThrPool workThr = null;

        //Event
        public event JoinEventHandler JoinEvent;
        public event DisconectedEventHandler DisconectedEvent;



        #region "Properties
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
            this.workThr = new ThrPool(10, 50);

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


                clusterAction((node) => { node.onClusterIncrease(nodeUrl); }, true);

                lock (cluster) { cluster.Add(nodeUrl, newPeer); }
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
                this.tracker.sendMessage(sender, msg);
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
                    catch
                    {
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




        public void onClusterIncrease(string peer) { if (JoinEvent != null) JoinEvent(peer); }
        public void onClusterDecrease(string peer) { if (DisconectedEvent != null) DisconectedEvent(peer); }
        public void onClusterMessage(string msg) { Console.WriteLine(msg); }
    }
}
