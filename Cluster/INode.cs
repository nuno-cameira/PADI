using Padi.SharedModel;
using System;

namespace Padi.Cluster
{
    public interface INode : IWorker, ICluster
    {
        CommunicationBehavior CommunicationBehavior{ get; }

        void switchCommunicationBehavior(CommunicationBehavior newBehavior);
    }
}
