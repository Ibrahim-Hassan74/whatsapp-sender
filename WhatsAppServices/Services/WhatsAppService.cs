using WhatsAppServices.Models;

namespace WhatsAppServices.Services
{

    public class WhatsAppService
    {
        private readonly HttpClient _http;
        public static string CurrentQR { get; set; } = "";
        public static bool IsConnected { get; set; } = false;

        public WhatsAppService(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("http://localhost:5000/");
        }

        public async Task<bool> SendMessage(string number, string message)
        {
            var response = await _http.PostAsJsonAsync("send", new { number, message });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CheckStatusAsync()
        {
            var response = await _http.GetAsync("status");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<StatusResponse>();
                return result.connected;
            }
            return false;
        }

        public async Task LogoutAsync()
        {
            await _http.PostAsync("logout", null);
        }

    }
}
