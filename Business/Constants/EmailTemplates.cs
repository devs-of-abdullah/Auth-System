namespace Business.Constants;

public static class EmailTemplates
{
    public const string VerificationEmailSubject = "Verify Your Email - AuthTemplate";
    
    public static string GetVerificationEmailBody(string code) => $@"
        <div style=""font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;"">
            <h2 style=""color: #333; text-align: center;"">Welcome to AuthTemplate!</h2>
            <p style=""font-size: 16px; color: #555;"">Thank you for registering. Please use the verification code below to complete your registration:</p>
            <div style=""background-color: #f4f4f4; padding: 20px; text-align: center; border-radius: 4px; margin: 25px 0;"">
                <span style=""font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #007bff;"">{code}</span>
            </div>
            <p style=""font-size: 14px; color: #888; text-align: center;"">This code will expire in 15 minutes.</p>
            <hr style=""border: 0; border-top: 1px solid #eee; margin: 20px 0;"">
            <p style=""font-size: 12px; color: #aaa; text-align: center;"">If you did not request this email, please ignore it.</p>
        </div>";

    public const string PasswordResetSubject = "Reset Your Password - AuthTemplate";

    public static string GetPasswordResetBody(string token) => $@"
        <div style=""font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;"">
            <h2 style=""color: #333; text-align: center;"">Password Reset Request</h2>
            <p style=""font-size: 16px; color: #555;"">We received a request to reset your password. Use the token below to proceed:</p>
            <div style=""background-color: #f4f4f4; padding: 20px; text-align: center; border-radius: 4px; margin: 25px 0;"">
                <span style=""font-size: 24px; font-weight: bold; color: #dc3545;"">{token}</span>
            </div>
            <p style=""font-size: 14px; color: #888; text-align: center;"">This token will expire in 1 hour.</p>
            <hr style=""border: 0; border-top: 1px solid #eee; margin: 20px 0;"">
            <p style=""font-size: 12px; color: #aaa; text-align: center;"">If you did not request a password reset, please ignore this email.</p>
        </div>";
}
