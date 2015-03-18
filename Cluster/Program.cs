﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (args.Length == 2)
            {
                Console.WriteLine("Creating Single Node " + args[0]);
                node = new Node(int.Parse(args[1]), false);
            }
            else
            {
                Console.WriteLine("Creating cluster Node " + args[0]);
                node = new Node(int.Parse(args[1]), false, args[2]);
            }



            node.JoinEvent += onJoinEvent;
            node.DisconectedEvent += onDisconectedEvent;

            Console.WriteLine("Server Ready!");
            Console.WriteLine("Type \"kill\" to close the server.");
            string input;
            do
            {
                input = System.Console.ReadLine();
                node.sendMessage(node.URL, input);

            } while (input != "kill");
           
        }

        private static void onDisconectedEvent(string url)
        {
            Console.WriteLine("onDisconectedEvent: " + url);
        }

        private static void onMessageEvent(string sender, string msg)
        {
            Console.WriteLine("onMessageEvent: " + sender + " " + msg);
        }

        private static void onJoinEvent(string url)
        {
            Console.WriteLine("onJoinEvent: " + url);
        }
    }
}