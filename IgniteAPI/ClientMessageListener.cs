using Apache.Ignite.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteAPI
{
    public class ClientMessageListener : IMessageListener<MessageWrapper>
    {
        public bool Invoke(Guid nodeId, MessageWrapper message)
        {
            // Process the received message
            // Example: Print the message content
            System.Console.WriteLine("Received message: " + message.Message);

            // Return true to continue listening for messages, or false to stop listening
            return true;
        }
    }
}
