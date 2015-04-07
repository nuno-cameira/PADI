using Padi.SharedModel;
using System;

namespace Padi.Cluster
{
    public interface INode : IWorker, ICluster
    {
        //Cluster Properties
        string URL { get; }
        int ID { get; }
        bool IsBusy { get; }

        //Cluster Actions
        void promote();
        bool doWork(int split, byte[] mapper, string className, string clientUrl);//"Submit" do Tracker
        void printStatus();

        //Cluster Events
        void onTrackerChange(string peer);
        void onClusterDecrease(string peer);
        void onClusterIncrease(string peer);

        //Worker Events
        void onSplitDone(string peer);
        void onSplitStart(string peer, int split, string clientUrl);

        //Tracker events
        void onJobDone(string clientUrl);
        void onJobReceived(int splits, byte[] mapper, string classname, string clientUrl);
    }
}
