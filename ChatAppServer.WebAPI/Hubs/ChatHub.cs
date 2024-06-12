using ChatAppServer.WebAPI.Context;
using ChatAppServer.WebAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppServer.WebAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext context;

        public ChatHub(AppDbContext context)
        {
            this.context = context;
        }

        public static Dictionary<string, Guid> Users = new();     

        // Kullanıcı Giriş Yaptığında Çalışır.
        public async Task Connect(Guid userId)
        {
            Users.Add(Context.ConnectionId, userId);
            var user = await context.Users.FindAsync(userId);
            if(user is not null)
            {
                user.Status = "online";
                await context.SaveChangesAsync();

                await Clients.All.SendAsync("Users", user);
            }
        }

        // Kullanıcı çıkış yaptığında çalışır.
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Users.TryGetValue(Context.ConnectionId, out Guid userId);
            Users.Remove(Context.ConnectionId);

            var user = await context.Users.FindAsync(userId);
            if (user is not null)
            {
                user.Status = "offline";
                await context.SaveChangesAsync();

                await Clients.All.SendAsync("Users", user);
            }
        }


    }
}
