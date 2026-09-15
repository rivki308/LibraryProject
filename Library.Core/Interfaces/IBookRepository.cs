using Library.Core.DTOs.Books;
using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Book?> GetByIdWithCategoryAsync(int id, CancellationToken cancellationToken);
    Task<(List<Book> Items, int TotalCount)> GetPagedAsync(BookQueryParameters queryParameters, CancellationToken cancellationToken);
    Task<bool> IsbnExistsAsync(string isbn, CancellationToken cancellationToken);
    Task AddAsync(Book book, CancellationToken cancellationToken);
    void Update(Book book);
    void Delete(Book book);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
