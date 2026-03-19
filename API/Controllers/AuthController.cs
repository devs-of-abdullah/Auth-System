using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using DTO.Auth;
using Business.Interfaces;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
     readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tokens = await _authService.Login(request);

        if (tokens == null)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(tokens);
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.VerifyEmail(request);

        if (!result)
            return BadRequest(new { message = "Invalid email or verification token." });

        return Ok(new { message = "Email successfully verified. You can now login." });
    }

    [HttpPost("resend-verification-email")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _authService.ResendVerificationEmail(request);

        return Ok(new { message = "If the email exists and is not verified, a new verification code has been sent." });
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tokens = await _authService.RefreshToken(request);

        if (tokens == null)
            return Unauthorized(new { message = "Invalid refresh request" });

        return Ok(tokens);
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _authService.ForgotPassword(request);
        
        return Ok(new { message = "If the email exists, a password reset token has been generated." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ResetPassword(request);

        if (!result)
            return BadRequest(new { message = "Invalid or expired token, or email does not match." });

        return Ok(new { message = "Password has been successfully reset. You can now login." });
    }

    [Authorize]
    [HttpPost("logout")]

    public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        await _authService.Logout(userId, request.RefreshToken);

        return NoContent();
    }
}
