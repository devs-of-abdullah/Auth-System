using System.ComponentModel.DataAnnotations;

namespace DTO.Auth
{
    public class VerifyEmailRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Verification code must be 4 digits.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Verification code must be numeric.")]
        public string Code { get; set; } = null!;
    }
}
