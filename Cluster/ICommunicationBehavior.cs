using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padi.Cluster
{
    public interface ICommunicationBehavior
    {
        bool tryPromote();
        void promote();
        //private void nodeAction(ClusterHandler onSucess, string url);
        //private void nodeAction(ClusterHandler onSucess, ClusterHandler onFail, string url);
        //private void clusterAction(ClusterHandler onSucess);
        void submit(int splits, byte[] mapper, string classname, string clientUrl);
        bool doWork(int split, byte[] code, string className, string clientUrl);
        void join(string nodeUrl);
        void disconect(string peer);
        //public void printStatus();
        void status();
        //bool assignTaskTo(INode node);
    }
}
