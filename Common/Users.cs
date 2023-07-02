using Microsoft.AspNetCore.SignalR;

namespace SignalR.Common
{
    public class Users
    {
        public static List<string> ConnectedUsers { get; set; } = new List<string>();        
    }
}
