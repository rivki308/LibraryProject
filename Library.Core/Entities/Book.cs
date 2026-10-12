using System.ComponentModel.DataAnnotations;
using Library.Core.Enums;

namespace Library.Core.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string ISBN { get; set; } = default!;
    public int PublishYear { get; set; }
    public BookStatus IsAvailable { get; set; } = BookStatus.Available;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    //[Timestamp]
    //public byte[] RowVersion { get; set; } = default!;
}
