namespace Library.Core.DTOs.Books;

public class BookResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string ISBN { get; set; } = default!;
    public int PublishYear { get; set; }
    public string IsAvailable { get; set; } = default!;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
}
