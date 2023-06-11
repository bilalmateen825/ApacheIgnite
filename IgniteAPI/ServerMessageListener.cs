using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Events;
using Apache.Ignite.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteAPI
{
    public class ServerMessageListener : IMessageListener<MessageWrapper>
    {
        Task task;

        public bool Invoke(Guid nodeId, MessageWrapper message)
        {
            // Process the received message
            // Example: Print the message content
            System.Console.WriteLine("Received message: " + message.Message);


            #region Verification of Communication among topics
            //Verification for sending messages from AdditionalComment to client
            //Sequence Client send message to server 
            //This task is acting like after server received message from client it send acknowledgment to client.
            //if (task == null)
            //{
            //    task = new Task(() =>
            //    {
            //        Ignition.GetIgnite().GetMessaging().SendOrdered(new MessageWrapper() { Topic = "OMS", Message = "This is it", AdditionalComment = "Initiation from server side" }, "OMS");
            //        Ignition.GetIgnite().GetMessaging().SendOrdered(new MessageWrapper() { Topic = "RMS", Message = "This is it", AdditionalComment = "Initiation from server side" }, "RMS");
            //    });

            //    task.Start();
            //}
            #endregion

            #region Clients Address

            // Return true to continue listening for messages, or false to stop listening
            // Get the list of connected client nodes
            //ICollection<IClusterNode> clientNodes = Ignition.GetIgnite().GetCluster().GetNodes();

            //List<string> strings = new List<string>();
            //// Iterate through the client nodes and print their addresses
            //foreach (IClusterNode clientNode in clientNodes)
            //{
            //    if (!clientNode.IsClient)
            //        continue;

            //    if (!strings.Contains(clientNode.Id.ToString()))
            //        strings.Add(clientNode.Id.ToString());

            //    Console.WriteLine("Connected client: " + clientNode.Addresses);
            //}
            #endregion

            return true;
        }
    }

    public class TopicMessageListener : IMessageListener<MessageWrapper>
    {

        private readonly string topic;

        public TopicMessageListener(string topic)
        {
            this.topic = topic;
        }

        Task task;

        public bool Invoke(Guid nodeId, MessageWrapper message)
        {
            // Process the received message
            // Example: Print the message content
            System.Console.WriteLine("Received message: " + message.Topic);

            //Verification for sending messages from AdditionalComment to client
            //Sequence Client send message to server 
            //This task is acting like after server received message from client it send acknowledgment to client.
            //if (task == null)
            //{
            //    task = new Task(() =>
            //    {
            //        Ignition.GetIgnite().GetMessaging().SendOrdered(new MessageWrapper() { Topic = "OMS", Message = "This is it", AdditionalComment = "Initiation from server side" }, "OMS");
            //        Ignition.GetIgnite().GetMessaging().SendOrdered(new MessageWrapper() { Topic = "RMS", Message = "This is it", AdditionalComment = "Initiation from server side" }, "RMS");
            //    });

            //    task.Start();
            //}

            // Return true to continue listening for messages, or false to stop listening
            return true;
        }
    }
}
