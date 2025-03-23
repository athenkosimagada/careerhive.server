using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace careerhive.application.Request;
public class ResendConfirmationRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
