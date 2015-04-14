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

namespace Padi.Cluster
{


    //Delegate to handle the received messages
    public delegate void JoinEventHandler(string newNode);
    //Delegate to handle the received messages
    public delegate void DisconectedEventHandler(string oldNode);

    //Delegate to handle the received messages
    public delegate void TrackerChangeEventHandler(string newTracker);

    //Delegate to handle the received messages
    public delegate void WorkStartEventHandler(string sender, int split, string clientUrl);
    //Delegate to handle the received messages
    public delegate void WorkEndEventHandler(string peer);

    //Delegate to handle the received messages
    public delegate void JobDoneEventHandler(string url);
    //Delegate to handle the received messages
    public delegate void NewJobEventHandler(int splits, byte[] mapper, string classname, string clientUrl);







    // A delegate to handle the calls to a node in the cluster
    public delegate void ClusterHandler(INode node);


    public class Node : MarshalByRefObject, INode
    {
        //Configuration constants
        static private readonly int THREADPOOL_THREAD_NUMBER = 10;
        static private readonly int THREADPOOL_BUFFER_SIZE = 50;

        //Node Variables
        private readonly TcpChannel channel = null;
        private readonly string url = null;
        private readonly int id;
        private bool haltWork = false; //Flag to stop work
        private ThrPool workThr = null;
        private List<Job> jobs; //job queue

        //Worker Variables
        private int splitWork;
        private string clientWork;

        //Cluster status Variables
        private Dictionary<string, INode> cluster = null;
        private INode tracker = null;
        private string trkUrl = "";

        //Event
        public event JoinEventHandler JoinEvent;
        public event DisconectedEventHandler DisconectedEvent;
        public event TrackerChangeEventHandler TrackerChangeEvent;
        public event WorkStartEventHandler WorkStartEvent;
        public event WorkEndEventHandler WorkEndEvent;
        public event JobDoneEventHandler JobDoneEvent;
        public event NewJobEventHandler NewJobEvent;




        #region "Properties
        public string URL { get { return this.url; } }

        public int ID { get { return this.id; } }

        public bool IsTracker { get { return this.trkUrl.Equals(this.url); } }

        //By busy we mean if it's working on a client split
        public bool IsBusy { get { return this.splitWork != -1; } }
        #endregion


        #region "Constructors"

        /*
        * Constructs a singe node and registers it.
        */
        public Node(int id, int port, bool ensureSecurity)
        {
            Console.WriteLine("Creating Node...");

            this.channel = new TcpChannel(port);
            this.url = "tcp://" + Util.LocalIPAddress() + ":" + port + "/W";
            this.trkUrl = this.url;
            this.cluster = new Dictionary<string, INode>();
            this.cluster.Add(this.URL, this);
            this.tracker = this;
            this.splitWork = -1;

            this.workThr = new ThrPool(THREADPOOL_THREAD_NUMBER, THREADPOOL_BUFFER_SIZE);
            this.id = id;
            this.jobs = new List<Job>();



            ChannelServices.RegisterChannel(this.channel, ensureSecurity);
            RemotingServices.Marshal(this, "Node", typeof(Node));

            Console.WriteLine("Created node w/ ID: " + id + " @ " + this.URL);

        }


        /*
         * Constructor a node and adds it to the provided cluster.
         */
        public Node(int id, int port, bool ensureSecurity, string clusterURL)
            : this(id, port, ensureSecurity)
        {
            INode cluster = (INode)Activator.GetObject(typeof(INode), clusterURL);

            Console.WriteLine("Joining cluster @ " + clusterURL);

            //Attempt to join the cluster
            cluster.join(this.URL);

        }


        #endregion


        #region "Cluster Actions"
        /*
        * Receives a Node's URL and adds it to the cluster.
        * It also returns information about all nodes on this cluster and the active tracker
        */
        public void join(string nodeUrl)
        {


            if (this.IsTracker)
            {
                //Creates node's proxy
                INode newPeer = (INode)Activator.GetObject(typeof(INode), nodeUrl);

                //Prepares cluster's view
                ClusterReport report = new ClusterReport();
                report.View = 1;//No use as for now
                report.Tracker = this.URL;
                report.Cluster = new List<string>(this.cluster.Keys);

                newPeer.setup(report);

                //Tell cluster to add this new node
                clusterAction((node) => { node.onClusterIncrease(nodeUrl); });
            }
            else
            {
                nodeAction((trk) => { trk.join(nodeUrl); }, this.trkUrl);
            }
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
        }



        /*
         * Computes which node of the cluster will be promoted to cluster. 
         * Once it computes it will call the node to promote it.
         */
        private void tryPromote()
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

            //Chek if node is this or a remote one
            if (lowestURL.Equals(this.URL))
                promote();
            else
                nodeAction((node) => { node.promote(); }, lowestURL);
        }

        /*
         * Promotes local node to a tracker one.
         */
        public void promote()
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
                    this.tracker = this;
                    this.trkUrl = this.url;
                    clusterAction((node) => { node.onTrackerChange(this.URL); });
                }

            }
        }


        /*
         * A safe operation to call remote methods of a specific node.
         * Like the alike 'clusterAction()' it automatically handles disconections.
         */
        private void nodeAction(ClusterHandler onSucess, string url)
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
                onSucess(node);
            }
            catch //remote server does not exist
            {
                //If call failed to tracker then we need a new one
                if (url == this.trkUrl)
                {
                    tryPromote();
                }
                else//If call failed to worker then report to tracker
                {
                    nodeAction((trk) => { trk.disconect(url); }, this.trkUrl);
                }
            }
        }




        /*
         * A safe operation to call remote methods of all node in the cluster
         * It offers the possibility to exclude the call of the local method
         */
        private void clusterAction(ClusterHandler onSucess)
        {
            List<string> disconected = new List<string>();


            foreach (KeyValuePair<string, INode> entry in cluster)
            {
                INode node = entry.Value;
                if (node == null)
                    node = (INode)Activator.GetObject(typeof(INode), entry.Key);

                this.workThr.AssyncInvoke(() =>
                {
                    try
                    {
                        onSucess(node);
                    }
                    catch
                    {

                        disconect(entry.Key);
                    }
                });
            }
        }


        /*
         * Handles node disconections
         */
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
                        });
                    }

                }

            }
            else
            {
                nodeAction((trk) => { trk.disconect(peer); }, this.trkUrl);

            }
        }


        #endregion


        #region "Worker"

        /*
         * 
         */
        public void submit(int splits, byte[] mapper, string classname, string clientUrl)
        {
            if (this.IsTracker)
            {

                onJobReceived(splits, mapper, classname, clientUrl);

                clusterAction((node) =>
                {
                    //Tracker wont work if there's Workers in the cluster
                    if (node.URL == this.URL)
                    {
                        return;
                    }

                    //Inform nodes about new job
                    node.onJobReceived(splits, mapper, classname, clientUrl);

                    //Assign split to node if possible
                    assignTaskTo(node);
                });
            }
            else
            {
                nodeAction((trk) => { trk.submit(splits, mapper, classname, clientUrl); }, this.trkUrl);
                return;
            }
        }

        private static EventWaitHandle halt =
     new EventWaitHandle(false, EventResetMode.AutoReset);


        public bool doWork(int split, byte[] code, string className, string clientUrl)
        {
            //Node is now doing work
            this.splitWork = split;
            this.clientWork = clientUrl;

            this.workThr.AssyncInvoke(() =>
            {
                //inform everyone we're starting to do work on this split
                clusterAction((node) => { node.onSplitStart(this.url, split, clientUrl); });

                IMapper mapper = Util.loadMapper(code, className);

                //Contact client and request content
                IClient client = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
                byte[] splitCByte = client.returnSplit(split);

                string splitContent = System.Text.Encoding.UTF8.GetString(splitCByte);

                Dictionary<string, string> groupedKeys = new Dictionary<string, string>();



                //Map each line
                using (StringReader reader = new StringReader(splitContent))
                {
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


                        object[] args = new object[] { line };

                        IList<KeyValuePair<string, string>> result = mapper.Map(line);

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

                var array = groupedKeys.Keys.ToArray();

                Array.Sort(array);

                string returnString = null;

                // Loop through keys.
                foreach (var key in array)
                {
                    returnString += key + ": [" + groupedKeys[key] + "]\n";
                }

                //Return the result to the client
                client.onSplitDone(returnString, split);


                //Node is available for new work
                this.splitWork = -1;

                //Report we're done with the task
                clusterAction((node) => { node.onSplitDone(this.url); });

            });
            return true;
        }



        public void freezeW(int id)
        {
            if (!this.IsTracker && id == this.ID)
            {
                Console.WriteLine("freezeW()");
                haltWork = true;
            }
            else
            {
                if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.ID == id) { node.freezeW(id); } });
                }
                else
                {
                    nodeAction((trk) => { trk.freezeW(id); }, this.trkUrl);
                }
            }
        }


        public void freezeC(int id)
        {
            if (this.IsTracker && id == this.ID)
            {
                Console.WriteLine("freezeC()");
                haltWork = true;
            }
            else
            {
                if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.ID == id) { node.freezeC(id); } });
                }
                else
                {
                    nodeAction((trk) => { trk.freezeC(id); }, this.trkUrl);
                }
            }
        }


        public void unFreezeW(int id)
        {
            if (!this.IsTracker && id == this.ID)
            {
                Console.WriteLine("unFreezeW()");
                this.haltWork = false;
                halt.Set();
            }
            else
            {
                if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.ID == id) { node.unFreezeW(id); } });
                }
                else
                {
                    nodeAction((trk) => { trk.unFreezeW(id); }, this.trkUrl);
                }
            }
        }


        public void unFreezeC(int id)
        {
            if (this.IsTracker && id == this.ID)
            {
                Console.WriteLine("unFreezeC()");
                this.haltWork = false;
                halt.Set();
            }
            else
            {
                if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.ID == id) { node.unFreezeC(id); } });
                }
                else
                {
                    nodeAction((trk) => { trk.unFreezeC(id); }, this.trkUrl);
                }
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
                if (this.IsTracker)
                {
                    clusterAction((node) => { if (node.ID == id) { node.slowW(id, time); } });
                }
                else
                {
                    nodeAction((trk) => { trk.slowW(id, time); }, this.trkUrl);
                }
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

        public void status()
        {
            if (this.IsTracker)
            {
                clusterAction((node) => { node.printStatus(); });
            }
            else
            {
                nodeAction((trk) => { trk.status(); }, this.trkUrl);
            }
        }



        #endregion


        #region "Tracker"





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

            //send event
            if (JoinEvent != null) { JoinEvent(peer); }
        }


        public void onClusterDecrease(string peer)
        {
            //TODO: Check if node was doing a job 
            //if so retask it

            lock (cluster) { cluster.Remove(peer); }
            if (DisconectedEvent != null) DisconectedEvent(peer);
        }


        public void onTrackerChange(string p)
        {
            Console.WriteLine("onTrackerChange");
            if (!this.IsTracker) { this.tracker = (INode)Activator.GetObject(typeof(INode), p); }
            if (TrackerChangeEvent != null) TrackerChangeEvent(p);
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
                    if (!j.hasSplits())
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


            //Send Event
            if (WorkEndEvent != null) WorkEndEvent(peer);


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
            //Send Event
            if (WorkStartEvent != null) WorkStartEvent(peer, split, clientUrl);
        }


        public void onJobDone(string clientUrl)
        {
            Console.WriteLine("onJobDone(" + clientUrl + ")");


            //Send Event
            if (JobDoneEvent != null) JobDoneEvent(clientUrl);
        }

        public void onJobReceived(int splits, byte[] mapper, string className, string clientUrl)
        {
            Console.WriteLine("onJobReceived(" + splits + ",mapper ," + clientUrl + ")");

            //Make a new job
            Job job = new Job(splits, mapper, className, clientUrl);
            jobs.Add(job);

            //Send Event
            if (NewJobEvent != null) NewJobEvent(splits, mapper, className, clientUrl);
        }
    }

        #endregion

}
