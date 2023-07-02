using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Messaging;
using System;
using Apache.Ignite.Core.Cache.Event;
using IgniteAPI;
using System.Reflection;
using System.Drawing;
using Ignite_Client_Server;
using System.ComponentModel;

namespace IgniteServer
{
    class IgniteServer
    {
        private const string CacheName = "myCache";
        
        static void Main(string[] args)
        {
            var server = new MessagingServer();

            Console.WriteLine("Server started. Press any key to exit...");

            // Start listening for messages
            server.StartListening();
        
            //for (int i = 0; i < 10000; i++)
            //{
            //    server.SendMessage("OMS", "How are you?");
            //    server.SendMessage("RMS", "How are you?");
            //}

            System.Console.WriteLine("Message sent from Server.");

            Console.ReadKey();
        }
    }
}
