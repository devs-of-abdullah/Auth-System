using System.ComponentModel.DataAnnotations;

namespace DTO.Auth
{
    public class VerifyEmailRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Token { get; set; } = null!;
    }
}
