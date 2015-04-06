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
        bool doWork(int split, byte[] mapper, string className, string clientUrl);//"Submit" do Tracker
        void freezeW(int id);
        void unFreezeW(int id);
        void freezeC(int id);
        void unFreezeC(int id);
        void status();
        void slowW(int id, int time);

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
