using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce;

namespace Padi.SharedModel
{
    static public class Util
    {
       static public string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }


       static public IMapper loadMapper(byte[] code, string className)
       {
           Assembly assembly = Assembly.Load(code);

           // Walk through each type in the assembly looking for our class
           foreach (Type type in assembly.GetTypes())
           {
               if (type.IsClass == true)
               {
                   if (type.FullName.EndsWith("." + className))
                   {
                       // create an instance of the object
                       return (IMapper)Activator.CreateInstance(type);
                   }
               }
           }

           throw (new System.Exception("could not invoke method"));
       }

       static public void Populate<T>(this T[] arr, T value)
       {
           for (int i = 0; i < arr.Length; i++)
           {
               arr[i] = value;
           }
       }
    }
}
