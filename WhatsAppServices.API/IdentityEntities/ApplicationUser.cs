using Microsoft.AspNetCore.Identity;

namespace WhatsAppServices.API.IdentityEntities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTimeOffset RefreshTokenExpirationDateTime { get; set; }
    }
}
