using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class TEST
    {

        
       static void Main2(string[] args)
       {
         Client client = new Client("tcp://localhost:30001/W", 10001);

           client.Submit("C:/temp/pl2000.txt",  "../../../result/",  2,  "CharCountMapper",  "../../.././LibMapperCharCount.dll");
       

           client.returnSplit(1);
           client.returnSplit(2);

           Console.ReadLine();
   }
    }

}