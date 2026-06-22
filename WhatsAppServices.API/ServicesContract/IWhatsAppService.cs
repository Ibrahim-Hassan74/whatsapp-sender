using WhatsAppServices.API.DTO;

namespace WhatsAppServices.API.ServicesContract
{
    public interface IWhatsAppService
    {
        Task<Result<SendMessageResponse>> SendMessageAsync(SendMessageRequest request);

        Task<Result<ConnectionStatusResponse>> CheckStatusAsync();

        Task<Result<bool>> LogoutAsync();

        Task<Result<bool>> UpdateQrAsync(string qr);

        Task<Result<QrCodeResponse>> GetLastQr();

        Task<Result<bool>> UpdateConnectionStatusAsync(bool isConnected);

        Task<Result<bool>> LogMessageAsync(string number, string message, bool isSent);

        //Task<Result<IEnumerable<MessageDto>>> GetMessagesAsync();
        Task<Result<PagedResponse<MessageDto>>> GetMessagesAsync(PaginationRequest request);
    }
}
