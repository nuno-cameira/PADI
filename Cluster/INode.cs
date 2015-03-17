using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Padi.SharedModel;

namespace Padi.Cluster
{
    public interface INode : IWorker, ITracker
    {
        string URL { get; }

        string join(string nodeUrl);
        void disconect(string peer);

        void onClusterMessage(string msg);
        void onClusterIncrease(string peer);
        void onClusterDecrease(string peer);


    }
}
