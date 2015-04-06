using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public interface IPuppetMaster
    {
        void createWorker(string[] input);
        void processWorker(string[] input);








        void reportJoinEvent(string sender, string newNode);
        void reportDisconectionEvent(string sender, string oldNode);
        void reportTrackerChangeEvent(string sender, string newTracker);

        void reportWorkStartEvent(string sender, string peer, int split, string clientUrl);
        void reportWorkEndEvent(string sender, string peer);

        void reportJobDoneEvent(string sender, string clientUrl);
        void reportNewJobEvent(string sender,  int splits, byte[] mapper, string classname, string clientUrl);

    }
}
