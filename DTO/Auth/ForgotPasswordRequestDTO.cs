using System.ComponentModel.DataAnnotations;

namespace DTO.Auth
{
    public class ForgotPasswordRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
