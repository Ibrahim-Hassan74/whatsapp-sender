namespace WhatsAppServices.API.DTO
{
    public class SendMessageResponse
    {
        public string Number { get; init; } = default!;
        public string Message { get; init; } = default!;
        public DateTime SentAt { get; init; }
    }
}
