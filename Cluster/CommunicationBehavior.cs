using Padi.Cluster;
using Padi.SharedModel;
using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadPool;

namespace Padi.Cluster
{

    // A delegate to handle the calls to a node in the cluster
    public delegate void ClusterHandler(INode node);


    /* 
    * Generic Communication Behavior of a Node
    */
    public abstract class CommunicationBehavior : MarshalByRefObject, ICommunicationBehavior
    {

        //Belonging Node of this CommunicationBehavior
        public INode belongingNode = null;

        //Configuration constants
        static private readonly int THREADPOOL_THREAD_NUMBER = 10;
        static private readonly int THREADPOOL_BUFFER_SIZE = 50;

        //Node Variables
        public readonly string url = null;
        public readonly int id;
        protected bool haltWork = false; //Flag to stop work
        internal ThrPool workThr = null;
        protected List<Job> jobs; //job queue
        protected List<ClusterHandler> freezer = new List<ClusterHandler>();

        //Worker Variables
        public int splitWork;
        protected string clientWork;

        //Cluster status Variables
        protected Dictionary<string, INode> cluster = null;
        protected INode tracker = null;
        public string trkUrl = "";



        #region "Properties
        public string URL { get { return this.url; } }

        public int ID { get { return this.id; } }

        public bool IsTracker { get { return this.trkUrl.Equals(this.url); } }

        /*public string[] clusterNodes
        {
            get
            {
                string[] nodesUrl = new string[cluster.Count];
                int i = 0;
                foreach (KeyValuePair<string, INode> entry in cluster)
                {
                    nodesUrl[i] = entry.Key;
                    i++;
                }
                return nodesUrl;
            }
        }*/

        public List<string> clusterNodes
        {
            get
            {
                List<string> nodesUrl = new List<string>();
                foreach (KeyValuePair<string, INode> entry in cluster)
                {
                    nodesUrl.Add(entry.Key);
                }
                return nodesUrl;
            }
        }

        //By busy we mean if it's working on a client split
        public bool IsBusy { get { return this.splitWork != -1; } }
        #endregion


        #region "Constructors"

        private int extractPortFromURL(string url)
        {
            int port = Convert.ToInt32(url.Split(':')[2].Split('/')[0]);
            return port;
        }

        // Constructor only for copying fields from other CommunicationBehavior class
        public CommunicationBehavior(CommunicationBehavior oldBehavior)
        {
            this.belongingNode = oldBehavior.belongingNode;

            this.url = oldBehavior.url;
            this.trkUrl = oldBehavior.trkUrl;
            //this.cluster = new Dictionary<string, INode>();
            this.cluster = oldBehavior.cluster;
            this.tracker = oldBehavior.tracker;
            this.splitWork = oldBehavior.splitWork;

            this.workThr = oldBehavior.workThr;
            this.id = oldBehavior.id;
            //this.jobs = new List<Job>();
            this.jobs = oldBehavior.jobs;
        }

        public CommunicationBehavior(INode node, int id, string url, bool ensureSecurity)
        {
            this.belongingNode = node;

            this.url = url;
            this.trkUrl = this.url;
            this.cluster = new Dictionary<string, INode>();
            this.cluster.Add(this.URL, node);
            this.tracker = node;
            this.splitWork = -1;

            this.workThr = new ThrPool(THREADPOOL_THREAD_NUMBER, THREADPOOL_BUFFER_SIZE);
            this.id = id;
            this.jobs = new List<Job>();
        }



        #endregion


        #region "Cluster Actions"

        public bool handshake(string nodeUrl)
        {
            return cluster.ContainsKey(nodeUrl);
        }


        public void setup(ClusterReport report)
        {

            this.trkUrl = report.Tracker;
            this.tracker = (INode)Activator.GetObject(typeof(INode), report.Tracker);

            Console.WriteLine("Joined cluster @ " + this.trkUrl);

            //Updates the current view with the cluster's one
            List<string> clus = new List<string>(report.Cluster);

            foreach (string s in clus)
            {
                if (s != this.URL) { onClusterIncrease(s); }
            }

            List<Job> jbs = new List<Job>(report.Jobs);
            foreach (Job j in jbs)
            {
                this.jobs.Add(j);
            }
        }


        protected abstract bool tryPromote(string deadUrl);


        public abstract void promote();


        /*
         * A safe operation to call remote methods of a specific node.
         * Like the alike 'clusterAction()' it automatically handles disconections.
         */

        protected bool nodeAction(ClusterHandler onSucess, string url)
        {
            //Obtain node
            INode node = null;
            if (url == this.trkUrl)
                node = this.tracker;
            else
                node = cluster[url];

            //Checks if proxy exists
            if (node == null)
                node = (INode)Activator.GetObject(typeof(INode), url);


            try
            {
                if (node.handshake(this.URL))
                {
                    onSucess(node);

                }
                return true;
            }
            catch (SocketException)//remote server does not exist
            {


                //If call failed to tracker then we need a new one
                if (url == this.trkUrl)
                {
                    freezer.Add(onSucess);

                    bool wasPromoted = tryPromote(url);
                    if (wasPromoted)
                    {
                        disconect(url);
                        if (node.handshake(this.URL))
                        {
                            onSucess(this.belongingNode);
                        }
                    }
                }
                else//If call failed to worker then report to tracker
                {
                    nodeAction((trk) =>
                    {
                        if (node.handshake(this.URL))
                        {
                            trk.CommunicationBehavior.disconect(url);
                        }
                    }, this.trkUrl
                        );
                }

                return false;
            }
        }




        /*
         * A safe operation to call remote methods of all node in the cluster
         * It offers the possibility to exclude the call of the local method
         */
        protected void clusterAction(ClusterHandler onSucess)
        {
            foreach (KeyValuePair<string, INode> entry in cluster)
            {
                this.workThr.AssyncInvoke(() =>
                {
                    nodeAction(onSucess, entry.Key);
                });
            }
        }




        #endregion


        #region "IWorker"


        public abstract void submit(int splits, byte[] mapper, string classname, string clientUrl);


        protected static EventWaitHandle halt =
     new EventWaitHandle(false, EventResetMode.AutoReset);


        public abstract bool doWork(int split, byte[] code, string className, string clientUrl);

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
        public abstract void join(string nodeUrl);

        /*
        * Handles node disconections
        */
        public abstract void disconect(string peer);


        public void freezeW(int id)
        {
            if (!this.IsTracker && id == this.ID)
            {
                belongingNode.switchCommunicationBehavior(new FrozenCommunicationBehavior(this));
                Console.WriteLine("freezeW()");
                haltWork = true;
            }
            else
            {
                clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { node.CommunicationBehavior.freezeW(id); } });

                /*if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { node.CommunicationBehavior.freezeW(id); } });
                }
                else
                {
                    nodeAction((trk) => { trk.CommunicationBehavior.freezeW(id); }, this.trkUrl);
                }*/
            }
        }


        public void freezeC(int id)
        {
            if (this.IsTracker && id == this.ID)
            {
                belongingNode.switchCommunicationBehavior(new FrozenCommunicationBehavior(this));
                Console.WriteLine("freezeC()");
                haltWork = true;
            }
            else
            {
                clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { node.CommunicationBehavior.freezeC(id); } });

                /*if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { node.CommunicationBehavior.freezeC(id); } });
                }
                else
                {
                    nodeAction((trk) => { trk.CommunicationBehavior.freezeC(id); }, this.trkUrl);
                }*/
            }
        }


        public void unFreezeW(int id)
        {
            if (!this.IsTracker && id == this.ID)
            {
                belongingNode.switchCommunicationBehavior(new NormalCommunicationBehavior(this));
                Console.WriteLine("unFreezeW()");
                this.haltWork = false;
                halt.Set();
            }
            else
            {
                clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { node.CommunicationBehavior.unFreezeW(id); } });

                /*if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { Console.WriteLine("DERP");  node.CommunicationBehavior.unFreezeW(id); } });
                }
                else
                {
                    nodeAction((trk) => { trk.CommunicationBehavior.unFreezeW(id); }, this.trkUrl);
                }*/
            }
        }


        public void unFreezeC(int id)
        {
            if (this.IsTracker && id == this.ID)
            {
                belongingNode.switchCommunicationBehavior(new NormalCommunicationBehavior(this));
                Console.WriteLine("unFreezeC()");
                this.haltWork = false;
                halt.Set();
            }
            else
            {
                clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { node.CommunicationBehavior.unFreezeC(id); } });

                /*if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { node.CommunicationBehavior.unFreezeC(id); } });
                }
                else
                {
                    nodeAction((trk) => { trk.CommunicationBehavior.unFreezeC(id); }, this.trkUrl);
                }*/
            }
        }


        public void slowW(int id, int time)
        {
            if (id == this.ID)
            {
                Console.WriteLine("slowW()");
                this.haltWork = true;
                System.Threading.Thread.Sleep(time * 1000);
                this.haltWork = false;
                halt.Set();
            }
            else
            {
                clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { node.slowW(id, time); } });

                /*if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.CommunicationBehavior.ID == id) { node.slowW(id, time); } });
                }
                else
                {
                    nodeAction((trk) => { trk.slowW(id, time); }, this.trkUrl);
                }*/
            }
        }


        public void printStatus()
        {
            const string endln = "\n";
            string status = "";

            //Job myJob=null;

            status += "******************************" + endln;
            status += "****      NODE STATUS     ****" + endln;
            status += endln;
            status += "ID: " + this.ID + endln;
            status += "URL: " + this.URL + endln;
            status += "Tracker: " + this.trkUrl + endln;
            status += "Cluster: " + this.cluster.Count + endln;
            foreach (KeyValuePair<string, INode> entry in this.cluster)
            {
                status += "   " + entry.Key + endln;
            }
            status += endln;
            status += "Working: " + this.IsBusy + endln;
            if (this.IsBusy)
            {
                status += "   Split: " + this.splitWork + endln;
                status += "   Client: " + this.clientWork + endln;
            }
            status += endln;
            status += "   Job Queue:" + endln;
            foreach (Job j in this.jobs)
            {
                status += "      " + j.Client + endln;
                status += "      " + j.hasSplits() + endln;
            }

            status += "******************************" + endln;
            status += "******************************" + endln;


            Console.WriteLine(status);
        }

        public abstract void status();



        #endregion



        #region "Tracker"


        protected abstract bool assignTaskTo(INode node);

        #endregion



        #region "Events"

        public void onClusterIncrease(string peer)
        {
            INode newNode = null;

            //Get proxy to new node 
            if (this.IsTracker) { newNode = (INode)Activator.GetObject(typeof(INode), peer); }

            //Add New node to cluster
            lock (cluster) { if (!cluster.ContainsKey(peer)) { cluster.Add(peer, newNode); } }

            //Check if theres available jobs for him   
            if (this.IsTracker) { nodeAction((node) => { assignTaskTo(node); }, peer); }
        }


        public void onClusterDecrease(string peer)
        {
            //TODO: Check if node was doing a job 
            //if so retask it

            lock (cluster) { cluster.Remove(peer); }
        }


        public void onTrackerChange(string tracker)
        {
            Console.WriteLine("onTrackerChange()");
            if (!this.IsTracker)
            {
                this.tracker = (INode)Activator.GetObject(typeof(INode), tracker);
            }

            List<ClusterHandler> unfreeze = new List<ClusterHandler>();
            foreach (ClusterHandler action in freezer)
            {
                if (nodeAction(action, tracker))
                    unfreeze.Add(action);
            }

            foreach (ClusterHandler doneAction in unfreeze)
            {

                freezer.Remove(doneAction);
            }

        }


        public void onSplitDone(string peer)
        {
            Console.WriteLine("onSplitDone(" + peer + ")");

            int s = -1;
            foreach (Job j in this.jobs)
            {

                if ((s = j.getSplit(peer)) != -1)
                {
                    j.splitDone(s);
                    if (j.isJobDone())
                    {
                        onJobDone(j.Client);
                    }
                }
            }

            if (this.IsTracker)
            {
                //Check if theres available jobs
                nodeAction((node) => { assignTaskTo(node); }, peer);
            }
        }


        public void onSplitStart(string peer, int split, string clientUrl)
        {
            Console.WriteLine("onSplitStart(" + peer + ", " + split + ", " + clientUrl + ")");

            //Tracker assigned split so no need to re-update
            if (!this.IsTracker)
            {
                foreach (Job j in this.jobs)
                {
                    if (j.Client == clientUrl)
                    {
                        j.assignSplit(peer, split);
                    }
                }
            }
        }


        public void onJobDone(string clientUrl)
        {
            Console.WriteLine("onJobDone(" + clientUrl + ")");
            if (this.IsTracker)
            {
                IClient client = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
                client.onJobDone();
            }
        }


        public void onJobReceived(int splits, byte[] mapper, string className, string clientUrl)
        {
            Console.WriteLine("onJobReceived(" + splits + ",mapper ," + clientUrl + ")");

            //Make a new job
            Job job = new Job(splits, mapper, className, clientUrl);
            jobs.Add(job);
        }

        #endregion



    }



    /* 
    * Simulates a Frozen Communication Behavior of a Node
    */
    class FrozenCommunicationBehavior : CommunicationBehavior
    {

        public FrozenCommunicationBehavior(CommunicationBehavior oldCommunicationBehavior) : base(oldCommunicationBehavior) { }

        public FrozenCommunicationBehavior(INode cluster, int id, string url, bool ensureSecurity) : base(cluster, id, url, ensureSecurity) { }


        protected override bool tryPromote(string deadUrl)
        {
            Console.WriteLine("FrozenCommunicationBehavior::tryPromote");
            throw new SocketException();
        }

        public override void promote()
        {
            Console.WriteLine("FrozenCommunicationBehavior::promote");
            throw new SocketException();
        }

        public override void submit(int splits, byte[] mapper, string classname, string clientUrl)
        {
            Console.WriteLine("FrozenCommunicationBehavior::submit");
            throw new SocketException();
        }

        public override bool doWork(int split, byte[] code, string className, string clientUrl)
        {
            Console.WriteLine("FrozenCommunicationBehavior::doWork");
            throw new SocketException();
        }

        public override void join(string nodeUrl)
        {
            Console.WriteLine("FrozenCommunicationBehavior::join");
            throw new SocketException();
        }

        public override void disconect(string peer)
        {
            Console.WriteLine("FrozenCommunicationBehavior::disconect");
            throw new SocketException();
        }

        public override void status()
        {
            Console.WriteLine("FrozenCommunicationBehavior::status");
            throw new SocketException();
        }

        protected override bool assignTaskTo(INode node)
        {
            Console.WriteLine("FrozenCommunicationBehavior::assignTaskTo");
            throw new SocketException();
        }

    }



    /* 
    * Standard Communication Behavior of a Node
    */
    class NormalCommunicationBehavior : CommunicationBehavior
    {

        public NormalCommunicationBehavior(CommunicationBehavior oldCommunicationBehavior) : base(oldCommunicationBehavior) { }

        public NormalCommunicationBehavior(INode cluster, int id, string url, bool ensureSecurity) : base(cluster, id, url, ensureSecurity) { }


        /*
         * Computes which node of the cluster will be promoted to cluster. 
         * Once it computes it will call the node to promote it.
         */
        protected override bool tryPromote(string deadUrl)
        {
            Console.WriteLine("NormalCommunicationBehavior::tryPromote");
            string lowestURL = this.URL;
            bool wasPromoted = false;
            cluster.Remove(deadUrl);
            foreach (KeyValuePair<string, INode> entry in cluster)
            {
                if (entry.Key.GetHashCode() < this.URL.GetHashCode())
                {
                    lowestURL = entry.Key;
                }
            }

            //Check if node is this or a remote one
            if (lowestURL.Equals(this.URL))
            {
                promote();
                wasPromoted = true;
            }
            else
            {
                nodeAction((node) => { node.CommunicationBehavior.promote(); }, lowestURL);
            }

            return wasPromoted;
        }


        /*
         * Promotes local node to a tracker one.
         */
        public override void promote()
        {
            //This operation will change several variables
            lock (this)
            {
                //Confirm we're not already the tracker
                if (!this.IsTracker)
                {
                    Console.WriteLine("What?\n Worker is evolving!");

                    //Instantiate all node's proxies
                    Dictionary<string, INode> newCluster = new Dictionary<string, INode>();
                    foreach (KeyValuePair<string, INode> entry in cluster)
                    {
                        INode node = (INode)Activator.GetObject(typeof(INode), entry.Key);
                        newCluster.Add(entry.Key, node);
                    }
                    this.cluster = newCluster;

                    //Updates cluster information about current tracker
                    this.tracker = belongingNode;
                    this.trkUrl = this.url;
                    clusterAction((node) => { node.CommunicationBehavior.onTrackerChange(this.URL); });

                    foreach (ClusterHandler action in freezer)
                        action(this.belongingNode);
                }
            }
        }


        public override void submit(int splits, byte[] mapper, string classname, string clientUrl)
        {
            if (this.IsTracker)
            {

                onJobReceived(splits, mapper, classname, clientUrl);

                clusterAction((node) =>
                {
                    //Tracker wont work if there's Workers in the cluster
                    if (node.CommunicationBehavior.URL == this.URL)
                    {
                        return;
                    }

                    //Inform nodes about new job
                    node.CommunicationBehavior.onJobReceived(splits, mapper, classname, clientUrl);

                    //Assign split to node if possible
                    assignTaskTo(node);
                });
            }
            else
            {
                nodeAction((trk) => { trk.CommunicationBehavior.submit(splits, mapper, classname, clientUrl); }, this.trkUrl);
                return;
            }
        }


        public override bool doWork(int split, byte[] code, string className, string clientUrl)
        {
            //Node is now doing work
            this.splitWork = split;
            this.clientWork = clientUrl;

            this.workThr.AssyncInvoke(() =>
            {
                //inform everyone we're starting to do work on this split
                clusterAction((node) => { node.CommunicationBehavior.onSplitStart(this.url, split, clientUrl); });

                IMapper mapper = Util.loadMapper(code, className);

                //Contact client and request content
                IClient client = (IClient)Activator.GetObject(typeof(IClient), clientUrl);

                byte[] splitCByte = client.returnSplit(split);

                client = null;

                Dictionary<string, string> groupedKeys = new Dictionary<string, string>();
                IList<KeyValuePair<string, string>> result;
                //Map each line
                using (var reader = new StreamReader(new MemoryStream(splitCByte), Encoding.ASCII))
                {
                    int j = 0;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {

                        if (haltWork)
                        {
                            Console.WriteLine("Halting...");
                            halt.WaitOne();
                            Console.WriteLine("Preparing thread...");
                            halt.Reset();
                            Console.WriteLine("Resuming...");
                        }
                        /*
                        //DEBUG SHIT
                        Console.WriteLine("Mapping line " + j);
                        j++;
                         */

                        result = mapper.Map(line);


                        foreach (KeyValuePair<string, string> p in result)
                        {
                            if (groupedKeys.ContainsKey(p.Key))
                            {
                                groupedKeys[p.Key] += " " + p.Value;
                            }
                            else
                            {
                                groupedKeys[p.Key] = p.Value;
                            }
                        }
                    }
                }

                string returnString = string.Join("\n", groupedKeys.Select(x => x.Key + ": [" + x.Value + "]"));

                //Return the result to the client
                client = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
                client.onSplitDone(returnString, split);


                //Node is available for new work
                this.splitWork = -1;

                //Report we're done with the task
                clusterAction((node) => { node.CommunicationBehavior.onSplitDone(this.url); });

            });
            return true;
        }


        /*
         * Adds a Node to the cluster.
         */
        public override void join(string nodeUrl)
        {
            if (this.IsTracker)
            {
                //Creates node's proxy
                INode newPeer = (INode)Activator.GetObject(typeof(INode), nodeUrl);

                //Prepares cluster's view
                ClusterReport report = new ClusterReport();
                report.View = 1;//No use as for now
                report.Tracker = this.URL;
                lock (this.cluster)
                {
                    report.Cluster = new List<string>(this.cluster.Keys);
                }
                report.Jobs = jobs;
                newPeer.CommunicationBehavior.setup(report);

                //Tell cluster to add this new node
                clusterAction((node) => { node.CommunicationBehavior.onClusterIncrease(nodeUrl); });
            }
            else
            {
                nodeAction((trk) => { trk.CommunicationBehavior.join(nodeUrl); }, this.trkUrl);
            }
        }

        public override void disconect(string peer)
        {
            Console.WriteLine("disconect::" + peer);
            if (this.IsTracker)
            {
                lock (cluster)
                {
                    if (cluster.ContainsKey(peer))
                    {
                        cluster.Remove(peer);
                        clusterAction((node) =>
                        {
                            node.CommunicationBehavior.onClusterDecrease(peer);
                        });
                    }
                }

            }
            else
            {
                nodeAction((trk) => { trk.CommunicationBehavior.disconect(peer); }, this.trkUrl);
            }

            lock (jobs)
            {
                int split = -1;
                foreach (Job j in jobs)
                {
                    if ((split = j.getSplit(peer)) != -1)
                    {
                        Console.WriteLine("Canceling assigned job " + split);

                        j.cancel(split);
                        break;
                    }
                }
            }
        }


        public override void status()
        {

            if (this.IsTracker)
            {
                clusterAction((node) => { node.CommunicationBehavior.printStatus(); });
            }
            else
            {
                nodeAction((trk) => { trk.CommunicationBehavior.status(); }, this.trkUrl);
            }
        }

        protected override bool assignTaskTo(INode node)
        {
            foreach (Job job in this.jobs)
            {

                if (job.hasSplits() && !node.CommunicationBehavior.IsBusy)
                {
                    node.CommunicationBehavior.doWork(job.assignSplit(node.CommunicationBehavior.URL), job.Mapper, job.ClassName, job.Client);
                    return true;
                }
            }
            return false;
        }
    }


}
