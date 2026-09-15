using System.ComponentModel.DataAnnotations;

namespace Library.Core.DTOs.Books;

public class BookUpdateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = default!;

    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Author { get; set; } = default!;

    [Required]
    [StringLength(20, MinimumLength = 10)]
    public string ISBN { get; set; } = default!;

    [Range(1000, 2100)]
    public int PublishYear { get; set; }

    [Required]
    public int CategoryId { get; set; }
}
