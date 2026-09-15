namespace Library.Core.Entities;

public class Member
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public bool IsAdmin { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
