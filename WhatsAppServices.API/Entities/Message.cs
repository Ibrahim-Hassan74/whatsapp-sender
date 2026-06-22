namespace WhatsAppServices.API.Entities
{
    public class Message
    {
        public Guid Id { get; set; }

        public string Number { get; set; } = default!;

        public string Content { get; set; } = default!;

        public bool IsSent { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? SentAt { get; set; }

        public string? Error { get; set; }
    }
}
