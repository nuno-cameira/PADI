using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Padi.Cluster
{
    delegate void ThrWork();
    class ThrPool
    {

        private Thread[] threads;
        private ThrWork[] actions;



        public ThrPool(int thrNum, int bufSize)
        {
            this.threads = new Thread[thrNum];
            this.actions = new ThrWork[bufSize];

            //Initiate all threads with basic behaviour
            for (int i = 0; i < thrNum; i++)
            {
                //Defining basic behaviour
                threads[i] = new Thread(new ThreadStart(() =>
                {
                    ThrWork action;
                    //Slave thread will work forever
                    while (true)
                    {
                        //Makes sure we're the only one watching the action list
                        lock (this.actions)
                        {
                            //FIFO Qeue: only watch to the top of it, wait if nothing
                            while (this.actions[0] == null)
                                Monitor.Wait(this.actions);

                            //Get action
                            action = this.actions[0];

                            //FIFO Qeue: Shift all elements one position
                            for (int ix = 1; ix < this.actions.Length; ix++)
                            {
                                this.actions[ix - 1] = this.actions[ix];
                            }
                            //FIFO Qeue: last element gets nulled
                            this.actions[actions.Length - 1] = null;
                        }
                        //Do the hard work
                        action.Invoke();
                    }
                }));//End defining basic behaviour

                //Start thread
                threads[i].Start();
            }
        }





        public void AssyncInvoke(ThrWork action)
        {
            bool flag = false;
            //Makes we're the only one messing with the buffer
            lock (this.actions)
            {
                //Search for an empty space to Qeue the Action
                for (int i = 0; i < this.actions.Length; i++)
                {
                    if (this.actions[i] == null)
                    {
                        this.actions[i] = action;
                        flag = true;//An action was qeued
                        break;
                    }
                }

                //If indeed an action was qeued, anounce it to an avaible thread
                if (flag)
                    Monitor.Pulse(this.actions);
            }
        }
    }//End class ThrPool
}
