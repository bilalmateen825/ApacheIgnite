using Apache.Ignite.Core;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Messaging;
using Apache.Ignite.Core.Log;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using SignalR.Common;
using IgniteAPI;

namespace Broadcaster.Classes.Ignite
{
    public class IgniteClient
    {
        private readonly IIgnite m_ignite;
        private readonly IMessageListener<MessageWrapper> m_messageListener;
        private readonly List<string> subscribedTopics;

        public IIgnite Ignite { get; set; }

        private readonly IHubContext<OMSHub> _hubContext;

        public IgniteClient(IHubContext<OMSHub> hubContext)
        {
            _hubContext = hubContext;
            Thread.Sleep(5000);
            m_ignite = Ignition.Start(new IgniteConfiguration
            {
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        //The port "47500" is the default port used for node discovery in Apache Ignite, but we can modify it if necessary.
                        Endpoints = new[] { "127.0.0.1:47500" }
                        //Endpoints = new[] { "127.0.0.1:47500..47501" }

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
            m_messageListener = new IgniteClientMessageListener(_hubContext);

            var messaging = m_ignite.GetMessaging();
            //m_ignite.GetEvents().ClusterGroup.Ignite.

            // Subscribe to the message topic
            SubscribeToTopic("OMS");
            SubscribeToTopic("RMS");
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

    public class CustomLogger : Apache.Ignite.Core.Log.ILogger
    {
        private readonly string _logFilePath;

        private readonly object _lockObject = new object();

        public CustomLogger(string logFile)
        {
            string logFilePath = string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, logFile);
            _logFilePath = logFilePath;
        }

        public void Log(Apache.Ignite.Core.Log.LogLevel level, string message, object[] args, IFormatProvider formatProvider, string category, string nativeErrorInfo, Exception ex)
        {
            if (string.IsNullOrEmpty(message) || ex == null || level != Apache.Ignite.Core.Log.LogLevel.Error)
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

        public bool IsEnabled(Apache.Ignite.Core.Log.LogLevel level)
        {
            // You can implement your own logic to determine if the specified log level is enabled
            // For now, let's assume all log levels are enabled
            return true;
        }
    }

    public class IgniteClientMessageListener : IMessageListener<MessageWrapper>
    {
        private readonly IHubContext<OMSHub> _hubContext;
        public IgniteClientMessageListener(IHubContext<OMSHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public bool Invoke(Guid nodeId, MessageWrapper message)
        {
            // Process the received message
            // Example: Print the message content
            //System.Console.WriteLine("Received message: " + message.Message);

            _hubContext.Clients.All.SendAsync("ReceiveMessage", message.Topic, message.Message);

            // Return true to continue listening for messages, or false to stop listening
            return true;
        }
    }
}
