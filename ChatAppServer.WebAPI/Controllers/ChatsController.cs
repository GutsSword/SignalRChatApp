using ChatAppServer.WebAPI.Context;
using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Hubs;
using ChatAppServer.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatAppServer.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IHubContext<ChatHub> hubContext;

        public ChatsController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            this.context = context;
            this.hubContext = hubContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users= await context.Users.OrderBy(x=>x.Name).ToListAsync();

            return Ok(users);
        }
        [HttpGet]
        public async Task<IActionResult> GetChats(Guid userId, Guid toUserId, CancellationToken cancellationToken)
        {
            var chat = await context.Chats.Where(x => x.UserId == userId && x.ToUserId == toUserId
            || x.ToUserId == userId && x.UserId == toUserId)
                .OrderBy(x => x.Date)
                .ToListAsync(cancellationToken);

            return Ok(chat);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto request, CancellationToken cancellationToken)
        {
            Chat chats = new()
            {
                UserId = request.UserId,
                Message = request.Message,
                ToUserId = request.ToUserId,
                Date = DateTime.Now,
            };
            await context.AddAsync(chats, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            string connectionId = ChatHub.Users.First(x => x.Value == chats.ToUserId).Key;

            await hubContext.Clients.Client(connectionId).SendAsync("Messages",chats);

            return Ok(chats);
        }
    }
}
