using Padi.SharedModel;
using System;

namespace Padi.Cluster
{
    public interface INode : IWorker
    {
        string URL { get; }
        void disconect(string peer);
        System.Collections.Generic.List<string> getCluster();
        string join(string nodeUrl);
        void onClusterDecrease(string peer);
        void onClusterIncrease(string peer);
        void onClusterMessage(string msg);
        void onTrackerChange(string p);
        void promote();
        void tryPromote();
        void sendMessage(string sender, string msg);
    }
}
