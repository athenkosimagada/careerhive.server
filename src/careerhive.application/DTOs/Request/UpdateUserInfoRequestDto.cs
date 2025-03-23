using System.ComponentModel.DataAnnotations;

namespace careerhive.application.Request;
public class UpdateUserInfoRequestDto
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    public string? ProfilePictureUrl { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = null!;
}
