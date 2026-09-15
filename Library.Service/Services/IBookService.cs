using Library.Core.DTOs;
using Library.Core.DTOs.Books;

namespace Library.Service.Services;

public interface IBookService
{
    Task<PagedResult<BookResponse>> GetPagedAsync(BookQueryParameters queryParameters, CancellationToken cancellationToken);
    Task<BookResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<BookResponse> CreateAsync(BookCreateRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, BookUpdateRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
