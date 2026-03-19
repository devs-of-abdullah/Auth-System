using DTO.Auth;

namespace Business.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDTO?> Login(LoginRequestDTO dto);
        Task<TokenResponseDTO?> RefreshToken(RefreshRequestDTO dto);
        Task Logout(int id, string refreshToken);
        Task<bool> VerifyEmail(VerifyEmailRequestDTO request);
        Task<bool> ForgotPassword(ForgotPasswordRequestDTO request);
        Task<bool> ResetPassword(ResetPasswordRequestDTO request);
        Task<bool> ResendVerificationEmail(ResendVerificationEmailRequestDTO request);
    }
}
