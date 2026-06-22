using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WhatsAppServices.Hubs;
using WhatsAppServices.Models;
using WhatsAppServices.Services;

namespace WhatsAppServices.Controllers
{
    public class WhatsAppController : Controller
    {
        private readonly WhatsAppService _waService;
        private readonly IHubContext<WhatsAppHub> _hubContext;

        public WhatsAppController(WhatsAppService waService, IHubContext<WhatsAppHub> hubContext)
        {
            _waService = waService;
            _hubContext = hubContext;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> UpdateQR([FromBody] QrDTO data)
        {
            WhatsAppService.CurrentQR = data.QrCode;
            WhatsAppService.IsConnected = false;
            await _hubContext.Clients.All.SendAsync("UpdateWhatsApp", data.QrCode, false);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] StatusDTO data)
        {
            WhatsAppService.IsConnected = data.IsReady;
            await _hubContext.Clients.All.SendAsync("UpdateWhatsApp", "", data.IsReady);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string number, string message)
        {
            try
            {
                var isSuccess = await _waService.SendMessage(number, message);

                if (isSuccess)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, error = "فشل في التواصل مع خدمة الواتساب (Node.js)" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentStatus()
        {
            var isConnected = await _waService.CheckStatusAsync();
            return Json(new { connected = isConnected });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _waService.LogoutAsync();
            return Ok();
        }

    }
}
