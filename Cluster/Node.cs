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
    //public delegate void ClusterHandler(INode node);


    public class Node : MarshalByRefObject, INode
    {

        //Node Variables
        private readonly TcpChannel channel = null;
        private readonly string url = null;
        private readonly int id;

        public CommunicationBehavior communicationBehavior = null;




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
            cluster.CommunicationBehavior.join(url);

        }


        #endregion



        #region "Properties

        public CommunicationBehavior CommunicationBehavior { get { return communicationBehavior; } }

        #endregion


        public void switchCommunicationBehavior(CommunicationBehavior oldBehavior, CommunicationBehavior newBehavior)
        {

        this.communicationBehavior = new FrozenCommunicationBehavior(this.communicationBehavior);

        }


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

        public bool handshake(string nodeUrl)
        {
            return communicationBehavior.handshake(nodeUrl);
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

        public void disconect(string url) 
        { 
            communicationBehavior.disconect(url); 
        }



        #endregion




    }
}
