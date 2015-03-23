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
    }
}
