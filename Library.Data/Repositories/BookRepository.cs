using Library.Core.DTOs.Books;
using Library.Core.Entities;
using Library.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Books
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book?> GetByIdWithCategoryAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<(List<Book> Items, int TotalCount)> GetPagedAsync(BookQueryParameters queryParameters, CancellationToken cancellationToken)
    {
        var query = _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParameters.Search))
        {
            var search = queryParameters.Search.Trim();
            query = query.Where(b =>
                b.Title.Contains(search) ||
                b.Author.Contains(search) ||
                b.ISBN.Contains(search));
        }

        if (queryParameters.CategoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == queryParameters.CategoryId.Value);
        }

        if (queryParameters.IsAvailable.HasValue)
        {
            var status = queryParameters.IsAvailable.Value
                ? Core.Enums.BookStatus.Available
                : Core.Enums.BookStatus.Loaned;
            query = query.Where(b => b.IsAvailable == status);
        }

        query = queryParameters.SortBy?.ToLowerInvariant() switch
        {
            "title" => queryParameters.Descending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "author" => queryParameters.Descending ? query.OrderByDescending(b => b.Author) : query.OrderBy(b => b.Author),
            "publishyear" => queryParameters.Descending ? query.OrderByDescending(b => b.PublishYear) : query.OrderBy(b => b.PublishYear),
            _ => query.OrderBy(b => b.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> IsbnExistsAsync(string isbn, CancellationToken cancellationToken)
    {
        return await _context.Books.AnyAsync(b => b.ISBN == isbn, cancellationToken);
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        await _context.Books.AddAsync(book, cancellationToken);
    }

    public void Update(Book book)
    {
        _context.Books.Update(book);
    }

    public void Delete(Book book)
    {
        _context.Books.Remove(book);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}
