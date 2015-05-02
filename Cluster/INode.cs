using Padi.SharedModel;
using System;

namespace Padi.Cluster
{
    public interface INode : IWorker, ICluster
    {
        CommunicationBehavior CommunicationBehavior{ get; }
    }
}
