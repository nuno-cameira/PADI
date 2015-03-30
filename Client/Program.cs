using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = null; ;

            if (args.Length != 1)
            {
                Console.WriteLine("Error: Missing arguments");
                return;
            }
            else
            {
                client = new Client(args[0]);
            }

            Console.WriteLine("Client Ready!");
            Console.WriteLine("Type \"kill\" to close the server.");
            string input;
            while (true)
            {
                input = System.Console.ReadLine();
                if (input == "kill")
                    break;

            }

            Console.WriteLine("Client killed.");
            Environment.Exit(0);
        }
    }
}
