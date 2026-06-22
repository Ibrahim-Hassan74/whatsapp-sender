namespace WhatsAppServices.API.DTO
{
    public class ConnectionStatusResponse
    {
        public bool IsConnected { get; init; }
        public DateTime LastUpdated { get; init; }
    }
}
