using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalR_Backend.Models
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, UserConnection> _connections = new ConcurrentDictionary<string, UserConnection>();

        public override Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            //string username = Context.GetHttpContext().Request.Query["username"];
            var userConnection = new UserConnection
            {
                //Username = username,
                ConnectionId = connectionId
            };
            _connections[connectionId] = userConnection;

            // Notify others that a user has connected
            //Clients.Others.SendAsync("UserConnected", userConnection);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            _connections.TryRemove(Context.ConnectionId, out var userConnection);

            // Notify others that a user has disconnected
            //Clients.Others.SendAsync("UserDisconnected", userConnection);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string data)
        {
            // Send message to all clients except the sender
            await Clients.Others.SendAsync("ReceiveMessage", data);
            // Send success message to the sender
            await Clients.Caller.SendAsync("ReceiveSuccess", "Operation completed successfully");
        }

        public Task AddToGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public Task RemoveFromGroup(string groupName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public Task SendMessageToGroup(string groupName, string message)
        {
            return Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }
    }

    public class UserConnection
    {
        public string Username { get; set; }
        public string ConnectionId { get; set; }
    }
}
