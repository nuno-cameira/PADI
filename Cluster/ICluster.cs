using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Padi.Cluster
{
    public interface ICluster
    {
        void join(string nodeUrl);
        void disconect(string peer);
        void freezeW(int id);
        void unFreezeW(int id);
        void freezeC(int id);
        void unFreezeC(int id);
        void status();
        void slowW(int id, int time);
    }
}
