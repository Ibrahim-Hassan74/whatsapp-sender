namespace WhatsAppServices.API.Entities
{
    public class WhatsAppSession
    {
        public Guid Id { get; set; }

        public bool IsConnected { get; set; }

        public DateTime LastUpdated { get; set; }

        public string? LastQrCode { get; set; }
    }
}
