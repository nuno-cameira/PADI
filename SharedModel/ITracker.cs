using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padi.SharedModel
{
    public interface ITracker
    {
        void sendMessage(string sender, string msg);
    }
}
