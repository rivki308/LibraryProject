namespace Library.Core.DTOs.Members;

public class MemberResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}
