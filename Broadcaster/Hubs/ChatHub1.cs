using Microsoft.AspNetCore.SignalR;

namespace Broadcaster.Hubs
{
    public class ChatHub1:Hub
    {
        public Task SendMessageAsync(string user,string message)
        {
            //if (!string.IsNullOrEmpty(user))
            //{
            //    return Clients.Client(user).SendAsync("ReceiveMessage", user, message);
            //}

            //Broadcast message
            //ReceiveMessage is MsgType
            return Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override Task OnConnectedAsync()
        {
            ConnectedUsers1.ConnectedUsers1.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectedUsers1.ConnectedUsers1.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
