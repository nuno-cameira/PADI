using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Padi.Cluster
{

    public class Job
    {
        private int splits;
        private byte[] mapper;
        private Dictionary<int, string> workAssignment;
        private List<int> splitsDone;
        private string clientUrl;


        internal Job(int splits, byte[] mapper, string clientUrl)
        {
            this.splits = splits;
            this.mapper = mapper;
            this.clientUrl = clientUrl;
            this.workAssignment = new Dictionary<int, string>();
            this.splitsDone = new List<int>();
        }



        /// <summary>
        /// Assigns a split to a node worker
        /// </summary>
        /// <remarks>
        /// Multi-threaded ready
        /// </remarks>
        /// <param name="node"></param>
        /// <returns> The number corresponding to the split assign to the node or -1 if no split was assign</returns>
        internal int assignSplit(string node)
        {
            int res = -1;
            lock (this)
            {
                if (workAssignment.Count <= splits)
                {

                    for (int i = 1; i <= splits; i++)
                    {
                        if (!workAssignment.ContainsKey(i))
                        {
                            workAssignment.Add(i, node);
                            res = i;
                            break;
                        }
                    }
                }
            }
            return res;
        }
        internal void splitDone(int split) { }
        internal bool isSplitDone(int split)
        {
            return splitsDone.Contains(split);
        }

        /// <summary>
        /// Checks if there's still splits to be worked
        /// </summary>
        /// <returns>True if there's still splits to assign else otherwise</returns>
        internal bool hasSplits()
        {
            bool res;
            lock (this)
            {
                //All splits in workAssignment are being worked on
                res = workAssignment.Count < splits;
            }

            return res;
        }

        internal byte[] Mapper { get { return this.mapper; } }

        internal string Client { get { return this.clientUrl; } }
    }




}
