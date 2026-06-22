using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhatsAppServices.API.DTO;
using WhatsAppServices.API.Enums;
using WhatsAppServices.API.Filter;
using WhatsAppServices.API.Models;
using WhatsAppServices.API.ServicesContract;

namespace WhatsAppServices.API.Controllers
{
    [ApiController]
    [CustomAuthorize]
    [Route("api/[controller]")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _service;

        public WhatsAppController(IWhatsAppService service)
        {
            _service = service;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var result = await _service.SendMessageAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var result = await _service.CheckStatusAsync();

            if (!result.IsSuccess)
                return StatusCode(500, result.Error);

            return Ok(result.Data);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _service.LogoutAsync();

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpGet("qr")]
        public async Task<IActionResult> GetQr()
        {
            var result = await _service.GetLastQr();

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Data);
        }
        [HttpPost("update-qr")]
        public async Task<IActionResult> UpdateQr([FromBody] QrUpdateRequest request)
        {
            var result = await _service.UpdateQrAsync(request.QrCode);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }
        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            var result = await _service.UpdateConnectionStatusAsync(request.IsConnected);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] PaginationRequest request)
        {
            var result = await _service.GetMessagesAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }
    }
}