using System.ComponentModel.DataAnnotations;

namespace Library.Core.DTOs.Loans;

public class LoanCreateRequest
{
    [Required]
    public int BookId { get; set; }
}
