using Microsoft.AspNetCore.SignalR;

namespace SignalR.Common
{
    public class OMSHub : Hub
    {
        public Task SendMessageAsync(string user, string message)
        {
            //if (!string.IsNullOrEmpty(user))
            //{
            //    return Clients.Client(user).SendAsync("ReceiveMessage", user, message);
            //    //ReceiveMessage is the method that will get triggered on All Connected clients.
            //}

            //Broadcast message
            //ReceiveMessage is MsgType
            return Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override Task OnConnectedAsync()
        {
            Users.ConnectedUsers.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Users.ConnectedUsers.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
