using Padi.SharedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMaster;
namespace Padi.Cluster
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Missing arguments");
                return;
            }

            Node node;





            if (args.Length == 2 )
            {
                node = new Node(int.Parse(args[0]), int.Parse(args[1]), false);
            }
            else
            {
                node = new Node(int.Parse(args[0]), int.Parse(args[1]), false, args[2]);
            }


      

            Console.WriteLine("Server Ready!");
            Console.WriteLine("Type \"kill\" to close the server.");
            string input;

            input = System.Console.ReadLine();
            int splits = Int16.Parse(input);
            node.submit(splits, null, "", args[1]);

            int t = 2;
            while (t != 0)
            {
                input = System.Console.ReadLine();
                node.freezeW(2);

                input = System.Console.ReadLine();
                node.unFreezeW(2);
                t--;

            }



            node = null;
            Console.WriteLine("Server killed.");
            Environment.Exit(0);
        }



      
    }
}
