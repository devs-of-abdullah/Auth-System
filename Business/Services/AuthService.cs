using DTO.Auth;
using Business.Interfaces;
using Business.Constants;
using Microsoft.Extensions.Configuration;
namespace Business.Services
{
    public class AuthService : IAuthService
    {
         readonly IUserRepository _userRepository;
         readonly ITokenService _tokenService;
         readonly IConfiguration _config;
         readonly IEmailService _emailService;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IConfiguration config, IEmailService emailService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _config = config;
            _emailService = emailService;
        }

        public async Task<TokenResponseDTO?> Login(LoginRequestDTO request)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || user.IsDeleted)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            if (!user.IsEmailVerified)
                throw new UnauthorizedAccessException("Please verify your email address before logging in.");

            var tokens = _tokenService.GenerateToken(user);

            int refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7");

            user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(tokens.RefreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshDays);
            user.RefreshTokenRevokedAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return tokens;
        }

        public async Task<TokenResponseDTO?> RefreshToken(RefreshRequestDTO request)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || user.IsDeleted)
                return null;

            if (user.RefreshTokenRevokedAt != null)
                return null;

            if (user.RefreshTokenExpiresAt == null ||
                user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                return null;

            if (string.IsNullOrEmpty(user.RefreshTokenHash) ||
                !BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash))
                return null;

            var tokens = _tokenService.GenerateToken(user);

            int refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7");

            user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(tokens.RefreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshDays);
            user.RefreshTokenRevokedAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return tokens;
        }

        public async Task Logout(int userId, string refreshToken)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.IsDeleted)
                return;

            if (user.RefreshTokenHash != null &&
                BCrypt.Net.BCrypt.Verify(refreshToken, user.RefreshTokenHash))
            {
                user.RefreshTokenRevokedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task<bool> VerifyEmail(VerifyEmailRequestDTO request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLower());
            if (user == null || user.IsDeleted) return false;

            if (user.IsEmailVerified) return true; 

            if (user.EmailVerificationTokenExpiresAt != null && user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
                return false;

            if (string.IsNullOrEmpty(user.EmailVerificationToken) || !BCrypt.Net.BCrypt.Verify(request.Code, user.EmailVerificationToken))
                return false;

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> ForgotPassword(ForgotPasswordRequestDTO request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLower());
            if (user == null || user.IsDeleted) return false;

            var resetToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

            user.ResetToken = BCrypt.Net.BCrypt.HashPassword(resetToken);
            user.ResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            var emailBody = EmailTemplates.GetPasswordResetBody(resetToken);
            await _emailService.SendEmailAsync(user.Email, EmailTemplates.PasswordResetSubject, emailBody);

            return true;
        }

        public async Task<bool> ResetPassword(ResetPasswordRequestDTO request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLower());
            if (user == null || user.IsDeleted) return false;

            if (user.ResetTokenExpiresAt == null || user.ResetTokenExpiresAt <= DateTime.UtcNow)
                return false;

            if (string.IsNullOrEmpty(user.ResetToken) || !BCrypt.Net.BCrypt.Verify(request.Token, user.ResetToken))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
            user.ResetToken = null;
            user.ResetTokenExpiresAt = null;
            
            user.RefreshTokenRevokedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return true;
        }
        public async Task<bool> ResendVerificationEmail(ResendVerificationEmailRequestDTO request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLower());
            if (user == null || user.IsDeleted || user.IsEmailVerified)
                return false;

            if (user.EmailVerificationTokenSentAt != null &&
                user.EmailVerificationTokenSentAt.Value.AddMinutes(1) > DateTime.UtcNow)
            {
                throw new InvalidOperationException("Please wait 1 minute before requesting a new verification code.");
            }

            var verificationToken = Random.Shared.Next(1000, 10000).ToString();

            user.EmailVerificationToken = BCrypt.Net.BCrypt.HashPassword(verificationToken);
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
            user.EmailVerificationTokenSentAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            var emailBody = EmailTemplates.GetVerificationEmailBody(verificationToken);
            await _emailService.SendEmailAsync(user.Email, EmailTemplates.VerificationEmailSubject, emailBody);

            return true;
        }
    }
}

