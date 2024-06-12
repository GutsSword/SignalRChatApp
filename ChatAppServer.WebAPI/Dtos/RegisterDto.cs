using System.ComponentModel.DataAnnotations;

namespace ChatAppServer.WebAPI.Dtos
{
    public record RegisterDto(string Name , IFormFile File);
}
