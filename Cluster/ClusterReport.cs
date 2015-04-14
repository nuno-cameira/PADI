using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padi.Cluster
{
    [Serializable]
    public class ClusterReport
    {
        public int View;
        public String Tracker;
        public IList<String> Cluster;
        public IList<Job> Jobs;
    }
}
