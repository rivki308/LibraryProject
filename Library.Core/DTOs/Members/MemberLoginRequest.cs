using System.ComponentModel.DataAnnotations;

namespace Library.Core.DTOs.Members;

public class MemberLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}
