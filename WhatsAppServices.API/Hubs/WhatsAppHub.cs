using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WhatsAppServices.API.Enums;

namespace WhatsAppServices.API.Hubs
{
    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    public class WhatsAppHub : Hub
    {

    }
}
