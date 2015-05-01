using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Padi.Cluster
{

    /*class CommunicationBehavior : ICommunicationBehavior 
    {
        public Node cluster;

        public CommunicationBehavior(Node cluster)
        {
            this.cluster = cluster;
        }
    }*/


    class FrozenCommunicationBehavior : ICommunicationBehavior
    {
        private bool tryPromote()
        {
            throw new SocketException();
        }
        public void promote()
        {
            throw new SocketException();
        }
        private void nodeAction(ClusterHandler onSucess, string url);
        private void nodeAction(ClusterHandler onSucess, ClusterHandler onFail, string url);
        private void clusterAction(ClusterHandler onSucess);
        public void submit(int splits, byte[] mapper, string classname, string clientUrl);
        public bool doWork(int split, byte[] code, string className, string clientUrl)
        {
            throw new SocketException();
        }
        public void join(string nodeUrl);
        public void disconect(string peer);
        //public void printStatus();
        public void status();
    }



    class NormalCommunicationBehavior : ICommunicationBehavior
    {
        private bool tryPromote()
        {
            string lowestURL = this.URL;
            bool wasPromoted = false;
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
            {
                promote();
                wasPromoted = true;
            }
            else
                nodeAction((node) => { node.promote(); }, lowestURL);

            return wasPromoted;
        }
        public void promote()
        {
            throw new SocketException();
        }
        private void nodeAction(ClusterHandler onSucess, string url);
        private void nodeAction(ClusterHandler onSucess, ClusterHandler onFail, string url);
        private void clusterAction(ClusterHandler onSucess);
        public void submit(int splits, byte[] mapper, string classname, string clientUrl);
        public bool doWork(int split, byte[] code, string className, string clientUrl)
        {
            throw new SocketException();
        }
        public void join(string nodeUrl);
        public void disconect(string peer);
        //public void printStatus();
        public void status();
    }


}
