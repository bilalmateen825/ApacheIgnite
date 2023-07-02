using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Client.Cache;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Messaging;
using System;
using System.Text;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Discovery.Tcp;
using IgniteAPI;
using Apache.Ignite.Core.Log;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;

namespace IgniteClient
{
    class IgniteClient
    {
        private const string CacheName = "myCache";

        #region Old Working Code
        static void Main(string[] args, string[] args1)
        {
            // Start Ignite client node
            //var cfg = new IgniteClientConfiguration { Endpoints = new[] { "127.0.0.1:47500..47501" } };

            Thread.Sleep(10000); // Waiting for AdditionalComment to Up
                                 //try
                                 //{
                                 //    using (var client = Ignition.StartClient(cfg))
                                 //{
                                 //    var cache = client.GetOrCreateCache<int, Person>(new CacheClientConfiguration
                                 //    {
                                 //        Name = CacheName,
                                 //        QueryEntities = new[] { new QueryEntity(typeof(int), typeof(Person)) }
                                 //    });

            //    ////Read
            //    // var a = cache[1];

            //    // Create
            //    cache[0] = new Person { Name = "Person 1", Age = 20 };
            //    cache[2] = new Person { Name = "Person 2", Age = 22 };

            //    Console.WriteLine("Data added to cache. Press any key to exit...");
            //    Console.ReadKey();
            //}
            //}
            //catch (IgniteClientException ex)
            //{
            //    Console.WriteLine($"Client connection failed: {ex.InnerException?.Message}");
            //}

            //foreach (IClientConnection connection in client.GetConnections())
            //{
            //    Console.WriteLine(connection.RemoteEndPoint);
            //}

            var client = Ignition.Start(new IgniteConfiguration
            {
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        //The port "47500" is the default port used for node discovery in Apache Ignite, but we can modify it if necessary.
                        Endpoints = new[] { "127.0.0.1:47500..47501" }

                        //Endpoints = new[] { "192.168.1.100:47500", "192.168.1.101:47500" } // List of server endpoints for discovery

                        //defining end points in array form 
                        /*
                         Endpoints = new[] 
                         {
                             "127.0.0.1:47500",
                             "127.0.0.1:47501",
                             "127.0.0.1:47502",
                             // Add more endpoints within the desired port range
                         }*/
                    },
                    SocketTimeout = TimeSpan.FromSeconds(0.3)
                },
                ClientMode = true // Setting client mode
            });

            var stName = client.GetCacheNames();

            var cache = client.GetOrCreateCache<int, Person>(new CacheConfiguration
            {
                Name = CacheName,
                //QueryEntities = new[] { new QueryEntity(typeof(int), typeof(Person)) }
            });

            IMessaging msg = client.GetMessaging();

            stName = client.GetCacheNames();
            cache.Put(3, new Person(1, "Person 99", 4));

            Console.Read();
        }
        #endregion

        static void Main(string[] args)
        {
            Thread.Sleep(10000);
            var client = new Client();

            client.SubscribeToTopic("OMS");
            client.SubscribeToTopic("RMS");

            // Send a message
            //client.SendMessage("How are you?", "OMS");

            //var cache = client.Ignite.GetOrCreateCache<int, Person>(CacheName);
            //cache.QueryContinuous()

            //for (int i = 0; i < 10000; i++)
            //{
            //    client.SendMessage("How are you?", "OMS");
            //}

            System.Console.WriteLine("Message sent from Client.");

            System.Console.ReadLine();
        }

    }

    public class Client
    {
        private readonly IIgnite m_ignite;
        private readonly IMessageListener<MessageWrapper> m_messageListener;
        private readonly List<string> subscribedTopics;

        public IIgnite Ignite { get; set; }

        public Client()
        {
            m_ignite = Ignition.Start(new IgniteConfiguration
            {
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        //The port "47500" is the default port used for node discovery in Apache Ignite, but we can modify it if necessary.
                        Endpoints = new[] { "127.0.0.1:47500..47501" }

                        //Endpoints = new[] { "192.168.1.100:47500", "192.168.1.101:47500" } // List of server endpoints for discovery

                        //Defining in array form 
                        /*
                         Endpoints = new[] 
                         {
                             "127.0.0.1:47500",
                             "127.0.0.1:47501",
                             "127.0.0.1:47502",
                             // Add more endpoints within the desired port range
                         }
                         */
                    },
                    //SocketTimeout = TimeSpan.FromSeconds(0.3)
                },

                ClientMode = true, // Setting client mode                
                Logger = new CustomLogger("logfile.log"),
            });

            subscribedTopics = new List<string>();

            // Create the message listener
            m_messageListener = new ClientMessageListener();

            var messaging = m_ignite.GetMessaging();
            //m_ignite.GetEvents().ClusterGroup.Ignite.

            // Subscribe to the message topic

            //messaging.LocalListen(messageListener, "RMS");
            //messaging.LocalListen(messageListener, "OMS");
        }

        public void SendMessage(string message, string topic)
        {
            // Get the messaging component
            var messaging = m_ignite.GetMessaging();

            // Send the message to the server node subscribed to the topic
            messaging.SendOrdered(new MessageWrapper() { Message = message, Topic = topic, AdditionalComment = "Server-Side" }, topic);
        }

        public void SubscribeToTopic(string topic)
        {
            // Get the messaging component
            var messaging = m_ignite.GetCluster().GetMessaging();

            // Subscribe to the specified topic
            messaging.LocalListen<MessageWrapper>(m_messageListener, topic);
            messaging.RemoteListen<MessageWrapper>(m_messageListener, topic);
            
            // Add the topic to the subscribed topics list
            subscribedTopics.Add(topic);
        }

        public void UnsubscribeFromTopic(string topic)
        {
            // Get the messaging component
            var messaging = m_ignite.GetMessaging();

            // Unsubscribe from the specified topic
            messaging.StopLocalListen(m_messageListener, topic);

            // Remove the topic from the subscribed topics list
            subscribedTopics.Remove(topic);
        }

        public void BroadcastMessage(string message)
        {
            // Get the messaging component
            var messaging = m_ignite.GetMessaging();

            // Send the message to each subscribed topic
            foreach (var topic in subscribedTopics)
            {
                messaging.Send(topic, new MessageWrapper { Message = message, Topic = topic });
            }
        }
    }

    public class CustomLogger : ILogger
    {
        private readonly string _logFilePath;

        private readonly object _lockObject = new object();

        public CustomLogger(string logFile)
        {
            string logFilePath = string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, logFile);
            _logFilePath = logFilePath;
        }

        public void Log(LogLevel level, string message, object[] args, IFormatProvider formatProvider, string category, string nativeErrorInfo, Exception ex)
        {
            if (string.IsNullOrEmpty(message) || ex == null || level != LogLevel.Error)
                return;

            if (!File.Exists(_logFilePath))
            {
                File.Create(_logFilePath);
            }

            lock (_lockObject)
            {
                // we need preferred logging library here
                // For example, using System.IO.File.AppendAllText:
                System.IO.File.AppendAllText(_logFilePath, $"{category} - {message}{Environment.NewLine}");
            }
        }

        public bool IsEnabled(LogLevel level)
        {
            // You can implement your own logic to determine if the specified log level is enabled
            // For now, let's assume all log levels are enabled
            return true;
        }
    }
}
