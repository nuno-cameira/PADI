using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padi.User
{
    public class Program
    {
        static void Main(string[] args)
        {           
            //DEBUG
            UserApplication userApp = new UserApplication();
            userApp.Init("entryURL");
            userApp.Submit("input", "output", 10, "class", "dll");
        }
    }
}
