using AutoMapper;
using Library.Core.DTOs;
using Library.Core.DTOs.Books;
using Library.Core.Entities;
using Library.Core.Interfaces;
using Library.Service.Services;

namespace Library.Service.Implementations;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IMapper _mapper;

    public BookService(IBookRepository bookRepository, ICategoryRepository categoryRepository, ILoanRepository loanRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _categoryRepository = categoryRepository;
        _loanRepository = loanRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<BookResponse>> GetPagedAsync(BookQueryParameters queryParameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _bookRepository.GetPagedAsync(queryParameters, cancellationToken);

        return new PagedResult<BookResponse>
        {
            Items = _mapper.Map<List<BookResponse>>(items),
            PageNumber = queryParameters.PageNumber,
            PageSize = queryParameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<BookResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdWithCategoryAsync(id, cancellationToken);
        return book is null ? null : _mapper.Map<BookResponse>(book);
    }

    public async Task<BookResponse> CreateAsync(BookCreateRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new InvalidOperationException("הקטגוריה שנבחרה אינה קיימת");

        if (await _bookRepository.IsbnExistsAsync(request.ISBN, cancellationToken))
        {
            throw new InvalidOperationException("קיים כבר ספר עם ISBN זהה");
        }

        var book = _mapper.Map<Book>(request);
        book.Category = category;

        await _bookRepository.AddAsync(book, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        var created = await _bookRepository.GetByIdWithCategoryAsync(book.Id, cancellationToken);
        return _mapper.Map<BookResponse>(created);
    }

    public async Task<bool> UpdateAsync(int id, BookUpdateRequest request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return false;
        }

        _ = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new InvalidOperationException("הקטגוריה שנבחרה אינה קיימת");

        book.Title = request.Title;
        book.Author = request.Author;
        book.ISBN = request.ISBN;
        book.PublishYear = request.PublishYear;
        book.CategoryId = request.CategoryId;

        _bookRepository.Update(book);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return false;
        }

        if (await _loanRepository.HasLoansAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("לא ניתן למחוק ספר שיש לו היסטוריית השאלות. ניתן להשאיר אותו במערכת גם אם אינו בשימוש כרגע.");
        }

        _bookRepository.Delete(book);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
