namespace WhatsAppServices.API.Entities
{
    public class MessageAttempt
    {
        public Guid Id { get; set; }

        public Guid MessageId { get; set; }

        public DateTime AttemptedAt { get; set; }

        public bool IsSuccess { get; set; }

        public string? Error { get; set; }

        public Message Message { get; set; } = default!;
    }
}
