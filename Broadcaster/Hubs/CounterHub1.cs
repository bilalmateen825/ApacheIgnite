using Microsoft.AspNetCore.SignalR;

namespace Broadcaster.Hubs
{
    public class CounterHub1 : Hub
    {
        public Task AddToTotal(string stUser, int value)
        {
            return Clients.All.SendAsync("CounterIncrement", stUser, value);
        }
    }
}
