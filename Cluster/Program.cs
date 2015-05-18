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
                node = new Node(int.Parse(args[0]), args[1], false);
            }
            else
            {
                node = new Node(int.Parse(args[0]), args[1], false, args[2]);
            }


      

            Console.WriteLine("Server Ready!");
           
            System.Console.ReadLine();
            Console.WriteLine("Server killed.");
           // Environment.Exit(0);
        }



      
    }
}
