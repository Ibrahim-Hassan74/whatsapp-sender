using System.ComponentModel.DataAnnotations;

namespace WhatsAppServices.API.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "{0} can't be blank")]
        [EmailAddress(ErrorMessage = "{0} Should be in proper email address format")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Required(ErrorMessage = "{0} can't be blank")]
        [DataType(DataType.Password)]
        [MinLength(10, ErrorMessage = "{0} should be at least 10 characters")]
        public string? Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}
