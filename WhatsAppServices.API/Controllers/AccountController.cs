using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using WhatsAppServices.API.DTO;
using WhatsAppServices.API.Enums;
using WhatsAppServices.API.IdentityEntities;
using WhatsAppServices.API.ServicesContract;

namespace WhatsAppServices.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtService jwtService, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _roleManager = roleManager;
        }

        private async Task EnsureRoleExistsAndAssignAsync(ApplicationUser user, string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });

            await _userManager.AddToRoleAsync(user, roleName);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                return BadRequest(new { message = "Login data cannot be null." });

            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
                return NotFound(new { message = "No account found with this email." });


            if (!user.EmailConfirmed)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You must confirm your email before logging in." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.RememberMe, true);

            if (result.Succeeded)
            {
                var response = await CreateSuccessLoginResponseAsync(user, loginDTO.RememberMe);
                return Ok(response);
            }
            else if (result.IsLockedOut)
            {
                string message = "Your account is temporarily locked due to multiple failed login attempts. Please try again later.";
                return StatusCode(StatusCodes.Status423Locked, new { message = message });
            }
            else if (result.IsNotAllowed)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "User is not allowed to login." });
            }
            else
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { message = "Incorrect email or password." });
            }
        }
        [HttpPost("generate-new-jwt-token")]
        public async Task<IActionResult> RefreshTokenAsync(TokenModel model)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.RefreshToken))
                return BadRequest(new { message = "Token and refresh token are required." });

            ClaimsPrincipal? principal;

            try
            {
                principal = _jwtService.GetPrincipalFromJwtToken(model.Token);
            }
            catch (SecurityTokenException ex)
            {
                return BadRequest(new { message = "Access token is invalid." });
            }

            if (principal is null)
                return BadRequest(new { message = "Access token is invalid." });

            var email = principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email claim is missing in token." });

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return NotFound(new { message = "User does not exist." });

            if (user.RefreshToken != model.RefreshToken ||
                user.RefreshTokenExpirationDateTime <= DateTimeOffset.UtcNow)
            {
                return BadRequest(new { message = "Refresh token is invalid or expired." });
            }


            bool rememberMe = bool.TryParse(principal.FindFirst("remember_me")?.Value, out var rm) && rm;

            var authResponse = await _jwtService.CreateJwtToken(user, rememberMe) as ApiSuccessResponse;


            // Rotate refresh token
            user.RefreshToken = authResponse?.RefreshToken;
            user.RefreshTokenExpirationDateTime = authResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(user);

            authResponse.Success = true;
            authResponse.StatusCode = 200;
            authResponse.Message = "Token refreshed successfully.";

            return Ok(authResponse);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync(string? email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpirationDateTime = DateTimeOffset.MinValue;
                    await _userManager.UpdateAsync(user);
                }
            }

            await _signInManager.SignOutAsync();

            return Ok(new { message = "Logged out successfully." });
        }


        private async Task<ApiSuccessResponse> CreateSuccessLoginResponseAsync(ApplicationUser user, bool rememberMe)
        {
            var tokenResponse = await _jwtService.CreateJwtToken(user, rememberMe) as ApiSuccessResponse;
            user.RefreshToken = tokenResponse?.RefreshToken;
            user.RefreshTokenExpirationDateTime = tokenResponse.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);
            tokenResponse.Success = true;
            tokenResponse.Message = "Login Successfully";
            tokenResponse.StatusCode = 200;
            return tokenResponse;
        }
    }
}
