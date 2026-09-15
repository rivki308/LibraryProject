namespace Library.Core.DTOs.Loans;

public class LoanResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = default!;
    public int MemberId { get; set; }
    public string MemberName { get; set; } = default!;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = default!;
}
