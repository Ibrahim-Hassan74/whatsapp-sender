using System.Security.Claims;
using WhatsAppServices.API.DTO;
using WhatsAppServices.API.IdentityEntities;

namespace WhatsAppServices.API.ServicesContract
{
    public interface IJwtService
    {
        Task<ApiResponse> CreateJwtToken(ApplicationUser user, bool rememberMe);
        ClaimsPrincipal? GetPrincipalFromJwtToken(string? token);
    }
}
