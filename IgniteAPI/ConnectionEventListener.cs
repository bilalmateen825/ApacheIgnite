using Apache.Ignite.Core;
using Apache.Ignite.Core.Events;
using System.ComponentModel;


namespace IgniteAPI
{
    public class ConnectionEventListener : IEventListener<CacheEvent>
    {
        private readonly IIgnite _ignite;

        public ConnectionEventListener(IIgnite ignite)
        {
            _ignite = ignite;
        }

        public bool Invoke(CacheEvent evt)
        {
            // Get the local node ID
            var localNodeId = _ignite.GetCluster().GetLocalNode().Id;

            // Handle the event when a node joins the cluster
            if (evt.Type == EventType.NodeJoined && evt.Node.Id != localNodeId)
            {
                // Logic to execute when a client node connects
                // This code will be executed on the server-side.
                Console.WriteLine("A client node has connected to the cluster.");
            }

            // Return 'true' to continue receiving events, 'false' to stop.
            return true;
        }
    }
}

