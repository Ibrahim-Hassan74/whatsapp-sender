namespace WhatsAppServices.API.DTO
{
    public class MessageDto
    {
        public string Number { get; init; } = default!;
        public string Message { get; init; } = default!;
        public bool IsSent { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
