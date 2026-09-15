using System.ComponentModel.DataAnnotations;

namespace Library.Core.DTOs.Members;

public class MemberRegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = default!;
}
