namespace AstroBackend.Application.DTOs
{
    public record RegisterRequest(string Email, string Password, string FullName);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(Guid Id, string Email, string FullName, string Role, string AccessToken, string RefreshToken);
    public record RefreshTokenRequest(string AccessToken, string RefreshToken);
    public record ChangePasswordRequest(string OldPassword, string NewPassword);

    /// <summary>Şifrə sıfırlama axınının 1-ci addımı — e-poçta sıfırlama linki göndərilir.</summary>
    public record ForgotPasswordRequest(string Email);

    /// <summary>Şifrə sıfırlama axınının 2-ci addımı — e-poçtla göndərilən token ilə yeni şifrə təyin edilir.</summary>
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
}
