using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Padi.Cluster
{
    [Serializable]
    public class Job
    {
        private int splits;
        private byte[] mapper;
        private string[] workAssignment;
        private bool[] workDone;
        private string clientUrl;
        private string className;

        internal Job(int splits, byte[] mapper, string className, string clientUrl)
        {
            this.splits = splits;
            this.mapper = mapper;
            this.clientUrl = clientUrl;
            this.workAssignment = new string[splits];
            Padi.SharedModel.Util.Populate<string>(this.workAssignment, null);

            this.workDone = new bool[splits];
            Padi.SharedModel.Util.Populate<bool>(this.workDone, false);
            this.className = className;
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
                for (int i = 0; i < this.workAssignment.Length; i++)
                {
                    if (this.workAssignment[i] == null)
                    {
                        this.workAssignment[i] = node;
                        res = i;
                        break;
                    }
                }
            }
            return res+1;
        }

        internal int assignSplit(string node, int split)
        {
            lock (this)
            {
                this.workAssignment[split-1] = node;
            }
            return -1;
        }



        internal void splitDone(int split)
        {
            this.workDone[split-1] = true;
        }
        internal bool isSplitDone(int split)
        {
            return this.workDone[split-1];
        }

        internal bool isJobDone()
        {
            foreach (bool splitDone in workDone) {
                if (!splitDone)
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Checks if there's still splits to be worked
        /// </summary>
        /// <returns>True if there's still splits to assign else otherwise</returns>
        internal bool hasSplits()
        {
            bool res = false;
            lock (this)
            {
                for (int i = 0; i < this.workAssignment.Length; i++)
                {
                    if (this.workAssignment[i] == null)
                    {
                        res = true;
                        break;
                    }
                }
            }
            return res;
        }

        internal byte[] Mapper { get { return this.mapper; } }

        internal string Client { get { return this.clientUrl; } }

        internal string ClassName { get { return this.className; } }




        internal int getSplit(string peer)
        {
            int res = -2;

            lock (this)
            {
                for (int i = 0; i < this.workAssignment.Length; i++)
                {
                    if (this.workAssignment[i] != null && this.workAssignment[i].Equals(peer))
                    {
                        
                        if (!isSplitDone(i+1))
                        {
                            Console.WriteLine("Peer is working on split :" + (i + 1));
                            res = i;
                            break;
                        }
                        else {
                            Console.WriteLine("Peer worked and finished split :" + (i + 1));
                        }
                    }
                }
            }


            return res+1;
        }



    }




}
