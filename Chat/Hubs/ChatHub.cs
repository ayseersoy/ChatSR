using Microsoft.AspNetCore.SignalR;
using Chat.Models;

namespace Chat.Hubs
{
    public class ChatHub : Hub
    {
        private static HashSet<string> ActiveUsers = new();
        private readonly AppDbContext _db;

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        public override Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext().Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                ActiveUsers.Add(username);
                Clients.All.SendAsync("UpdateActiveUsers", ActiveUsers.ToList());
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.GetHttpContext().Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                ActiveUsers.Remove(username);
                Clients.All.SendAsync("UpdateActiveUsers", ActiveUsers.ToList());
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string sender, string receiver, string message)
        {
            var chatMessage = new ChatMessage
            {
                Sender = sender,
                Receiver = receiver,
                Text = message,
                Timestamp = DateTime.Now
            };

            _db.ChatMessages.Add(chatMessage);
            _db.SaveChanges();

            await Clients.All.SendAsync("ReceiveMessage", sender, receiver, message);
        }

        private void SendActiveUsers()
        {
            var currentUser = Context.GetHttpContext().Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUser)) return;

            var allUsers = _db.ChatMessages
                             .Where(m => m.Sender == currentUser || m.Receiver == currentUser)
                             .Select(m => m.Sender == currentUser ? m.Receiver : m.Sender)
                             .Distinct()
                             .ToList();

            Clients.All.SendAsync("UpdateActiveUsers", allUsers, ActiveUsers.ToList());
        }


    }
}
