using System.ComponentModel.DataAnnotations;

namespace WhatsAppServices.API.DTO
{
    public class SendMessageRequest
    {
        [Required(ErrorMessage = "Number can't be blank")]
        [RegularExpression(@"^20(10|11|12|15)\d{8}$",
            ErrorMessage = "Invalid Egyptian phone number. Example: 2010xxxxxxxx")]
        public string Number { get; init; } = default!;

        [Required(ErrorMessage = "Message can't be blank")]
        [StringLength(3000, MinimumLength = 1,
            ErrorMessage = "Message must be between 1 and 1000 characters")]
        public string Message { get; init; } = default!;
    }
}