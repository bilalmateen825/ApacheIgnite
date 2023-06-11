using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Event;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Events;
using Apache.Ignite.Core.Messaging;
using IgniteAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ignite_Client_Server
{
    public class MessagingServer
    {
        private const string CacheName = "myCache";
        private readonly IIgnite m_ignite;
        private readonly IMessageListener<MessageWrapper> messageListener;
        private readonly Dictionary<string, IMessageListener<MessageWrapper>> topicListeners;

        public MessagingServer()
        {
            m_ignite = Ignition.Start(GetConfiguration());
            m_ignite.GetCluster().SetActive(true); // Activate the cluster
            m_ignite.ClientReconnected += OnClientConnect;
            
            //ignite.GetCluster().DisableWal(CacheName);
            var cache = m_ignite.GetOrCreateCache<int, Person>(new CacheConfiguration
            {
                Name = CacheName,
                QueryEntities = new[] { new QueryEntity(typeof(int), typeof(Person)) }
            });

            //var a = cache[1];
            var qry = new ContinuousQuery<int, Person>(new LocalListener(), new RemoteFilter());

            cache.QueryContinuous(qry);
            //ignite.GetCluster().EnableWal(CacheName);

            topicListeners = new Dictionary<string, IMessageListener<MessageWrapper>>();

            messageListener = new ServerMessageListener();
        }

        public IgniteConfiguration GetConfiguration()
        {
            var endpoints = new List<string>();
            int startPort = 47500;
            //int endPort = 47509;
            int endPort = 47501;

            for (int port = startPort; port <= endPort; port++)
            {
                string endpoint = $"127.0.0.1:{port}";
                endpoints.Add(endpoint);
            }

            string directoryPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/ssd/storage";
            IgniteConfiguration configuration = new IgniteConfiguration
            {
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        //Endpoints = new[] { "127.0.0.1:47500..47509" }
                        Endpoints = endpoints.ToArray()
                        //Endpoints = new[] { "127.0.0.1:10800" }
                    }
                },

                CommunicationSpi = new TcpCommunicationSpi { IdleConnectionTimeout = TimeSpan.FromSeconds(5) },

                //https://ignite.apache.org/docs/latest/persistence/native-persistence
                DataStorageConfiguration = new Apache.Ignite.Core.Configuration.DataStorageConfiguration()
                {
                    StoragePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\ssd\\storage",
                    //StoragePath = "/ssd/storage",
                    DefaultDataRegionConfiguration = new Apache.Ignite.Core.Configuration.DataRegionConfiguration()
                    {
                        Name = "Default_Region",
                        PersistenceEnabled = true,

                        //by setting InitialSize we are avoiding frequent disk space allocation and improve the efficiency of data region creation and subsequent storage operations.
                        InitialSize = 100 * 1024 * 1024, // Set the initial size of the data region

                        MaxSize = 10L * 1024 * 1024 * 1024, // Maximum region size in bytes (e.g., 10GB)                       
                        PageEvictionMode = Apache.Ignite.Core.Configuration.DataPageEvictionMode.Disabled, // Dont purge data
                        CheckpointPageBufferSize = 100 * 1024 * 1024,
                    },

                    WalFlushFrequency = TimeSpan.FromSeconds(30),
                    CheckpointFrequency = TimeSpan.FromMinutes(15),
                    WalMode = Apache.Ignite.Core.Configuration.WalMode.Fsync, //WalMode.LogOnly,
                    WalSegmentSize = 64 * 1024 * 1024,
                    MetricsEnabled = true
                }
            };

            return configuration;
        }

        public void StartListening()
        {
            // Get the messaging component
            var messaging = m_ignite.GetMessaging();

            // Subscribe to the message topic
            //messaging.LocalListen(messageListener,"topic");

            //messaging.RemoteListen(messageListener, "RMS");
            //messaging.RemoteListen(messageListener, "OMS");


            SubscribeToTopic("RMS");
            SubscribeToTopic("OMS");
        }

        private void OnClientConnect(object? sender, Apache.Ignite.Core.Lifecycle.ClientReconnectEventArgs e)
        {

        }

        public class LocalListener : ICacheEntryEventListener<int, Person>
        {
            private bool isUpdatingCache = false;

            public void OnEvent(IEnumerable<ICacheEntryEvent<int, Person>> evts)
            {
                foreach (var evt in evts)
                {
                    if (!isUpdatingCache)
                    {
                        Console.WriteLine($"Intercepted request from client: {evt.EventType} [Key={evt.Key}, Value={evt.Value}]");
                        var cache = Ignition.GetIgnite().GetOrCreateCache<int, Person>(CacheName);
                        Person p = evt.Value;

                        p.Name = "Modified-Data";
                        isUpdatingCache = true;
                        cache.Put(evt.Key, p); // Update the cache entry with the modified value
                        isUpdatingCache = false;
                    }
                }
            }
        }

        public class RemoteFilter : ICacheEntryEventFilter<int, Person>
        {
            public bool Evaluate(ICacheEntryEvent<int, Person> evt)
            {
                return true;
            }
        }
       
        void DetetectClientsNodes()
        {
            // Get the event storage from the Ignite instance
            var events = m_ignite.GetEvents();

            // Register the event listener
            var listener = new ConnectionEventListener(m_ignite);
            events.LocalListen(listener, EventType.NodeJoined);
        }

        public void SendMessage(string stTopic, string message)
        {
            // Get the messaging component
            var messaging = m_ignite.GetMessaging();

            // Send the message to all nodes subscribed to the topic
            messaging.SendOrdered(stTopic, new MessageWrapper() { Message = message, Topic = stTopic, AdditionalComment = "Client-Side" });
        }

        public void SubscribeToTopic(string topic)
        {
            // Get the messaging component
            var messaging = m_ignite.GetMessaging();

            // Create a new message listener for the topic
            var topicListener = new TopicMessageListener(topic);

            // Subscribe to the specified topic
            messaging.LocalListen<MessageWrapper>(topicListener, topic);

            // Add the topic listener to the dictionary
            topicListeners.Add(topic, topicListener);
        }

        public void UnsubscribeFromTopic(string topic)
        {
            if (topicListeners.TryGetValue(topic, out var topicListener))
            {
                // Get the messaging component
                var messaging = m_ignite.GetMessaging();

                // Unsubscribe from the specified topic
                messaging.StopLocalListen(topicListener, topic);

                // Remove the topic listener from the dictionary
                topicListeners.Remove(topic);
            }
        }
    }
}
