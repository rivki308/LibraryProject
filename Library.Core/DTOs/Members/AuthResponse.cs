namespace Library.Core.DTOs.Members;

public class AuthResponse
{
    public string Token { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public MemberResponse Member { get; set; } = default!;
}
