namespace AstroBackend.Application.DTOs
{
    public record RegisterRequest(string Email, string Password, string FullName);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(Guid Id, string Email, string FullName, string Role, string AccessToken, string RefreshToken);
    public record RefreshTokenRequest(string AccessToken, string RefreshToken);
    public record ChangePasswordRequest(string OldPassword, string NewPassword);
}
