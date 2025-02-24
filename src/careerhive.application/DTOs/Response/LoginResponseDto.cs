namespace careerhive.application.DTOs.Response;
public class LoginResponseDto
{
    public string TokenType { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = null!;
}
