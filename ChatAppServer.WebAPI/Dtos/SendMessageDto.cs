namespace ChatAppServer.WebAPI.Dtos
{
    public record SendMessageDto
    {
        public Guid UserId { get; set; }
        public Guid ToUserId { get; set; }
        public string Message { get; set; }
    }
}
