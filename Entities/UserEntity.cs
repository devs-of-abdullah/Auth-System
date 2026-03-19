namespace Entities;

public class UserEntity
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!; // admin, member

    public bool IsDeleted { get; set; }
 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    public string? RefreshTokenHash { get; set; } 
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime? RefreshTokenRevokedAt { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiresAt { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public DateTime? EmailVerificationTokenSentAt { get; set; }
}
