using System.ComponentModel.DataAnnotations;

namespace Library.Core.DTOs.Categories;

public class CategoryRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = default!;
}
