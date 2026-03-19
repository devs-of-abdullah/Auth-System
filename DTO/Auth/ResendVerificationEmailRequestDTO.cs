using System.ComponentModel.DataAnnotations;

namespace DTO.Auth;

public class ResendVerificationEmailRequestDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
