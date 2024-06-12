using ChatAppServer.WebAPI.Context;
using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using GenericFileService.Files;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAppServer.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext context;

        public AuthController(AppDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterDto request, CancellationToken cancellationToken)
        {
            // Kullanıcı ismi kontrolü
            bool isNameExist = await context.Users.AnyAsync(x => x.Name == request.Name, cancellationToken );
            if (isNameExist) 
            {
                return BadRequest(new
                {
                    Message = "Bu Kullanıcı Adı Zaten Mevcut."
                });
            }

            
            string avatar = FileService.FileSaveToServer(request.File, "wwwroot/avatar/");

            // Kullanıcı oluşturma
            User user = new()
            {
                Name = request.Name,
                Avatar = avatar,
            };


            await context.Users.AddAsync(user,cancellationToken);
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Login(string name, CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(x=>x.Name == name, cancellationToken);

            if(user is null)
            {
                return BadRequest(new
                {
                    Message = "Kullanıcı Bulunamadı."
                });
            }
            // User giriş yaptığında Online gözükür. 
            // Çıkış işlemi SignalR ' ın kendi metodu ile tespit edilecek.
            user.Status = "online";
            await context.SaveChangesAsync(cancellationToken);
            return Ok(user);
        }
    }
}
