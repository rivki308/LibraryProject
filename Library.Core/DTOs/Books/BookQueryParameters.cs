namespace Library.Core.DTOs.Books;

public class BookQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsAvailable { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
