namespace Library.Core.Entities;

public class Library
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
