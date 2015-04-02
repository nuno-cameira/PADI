using Padi.SharedModel;
using System;

namespace Padi.Cluster
{
    public interface INode : IWorker
    {
        //Cluster Properties
        string URL { get; }
        int ID { get; }
        bool IsBusy { get; }

        //Cluster Actions
        ClusterReport join(string nodeUrl);
        void disconect(string peer);
        void promote();
        bool doWork(int split, byte[] mapper, string clientUrl);//"Submit" do Tracker

        //Cluster Events
        void onTrackerChange(string peer);
        void onClusterDecrease(string peer);
        void onClusterIncrease(string peer);

        //Worker Events
        void onSplitDone(string peer);
        void onSplitStart(string peer, int split, string clientUrl);

        //Tracker events
        void onJobDone(string clientUrl);
        void onJobReceived(int splits, byte[] mapper, string clientUrl);
    }
}
