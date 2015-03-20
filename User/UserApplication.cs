using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padi.User
{
    public class UserApplication
    {
        public Client localClient;

        public void Init( string EntryURL )
        {
            localClient = new Client( EntryURL );
        }
    }
}
