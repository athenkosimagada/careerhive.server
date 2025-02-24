namespace careerhive.application.DTOs;
public class UserInfoDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }

    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }
}
