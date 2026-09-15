using AutoMapper;
using Library.Core.DTOs.Loans;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;
using Library.Service.Services;
using Microsoft.EntityFrameworkCore;

namespace Library.Service.Implementations;

public class LoanService : ILoanService
{
    private const int DefaultLoanDays = 14;

    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public LoanService(ILoanRepository loanRepository, IBookRepository bookRepository, IMapper mapper)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<LoanResponse> CreateAsync(int memberId, LoanCreateRequest request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken)
            ?? throw new KeyNotFoundException("הספר המבוקש אינו קיים");

        if (book.IsAvailable != BookStatus.Available)
        {
            throw new InvalidOperationException("הספר כבר מושאל ואינו זמין כרגע");
        }

        book.IsAvailable = BookStatus.Loaned;

        var loan = new Loan
        {
            BookId = book.Id,
            MemberId = memberId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(DefaultLoanDays),
            Status = LoanStatus.Active
        };

        _bookRepository.Update(book);
        await _loanRepository.AddAsync(loan, cancellationToken);

        try
        {
            await _bookRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new LoanConcurrencyException("הספר נתפס על ידי משתמש אחר. נסה שוב");
        }

        var created = await _loanRepository.GetByIdWithDetailsAsync(loan.Id, cancellationToken);
        return _mapper.Map<LoanResponse>(created);
    }

    public async Task<List<LoanResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var loans = await _loanRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<LoanResponse>>(loans);
    }

    public async Task<LoanResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var loan = await _loanRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return loan is null ? null : _mapper.Map<LoanResponse>(loan);
    }

    public async Task<List<LoanResponse>> GetMyLoansAsync(int memberId, CancellationToken cancellationToken)
    {
        var loans = await _loanRepository.GetByMemberIdAsync(memberId, cancellationToken);
        return _mapper.Map<List<LoanResponse>>(loans);
    }

    public async Task<LoanResponse> ReturnAsync(int loanId, int memberId, CancellationToken cancellationToken)
    {
        var loan = await _loanRepository.GetByIdWithDetailsAsync(loanId, cancellationToken)
            ?? throw new KeyNotFoundException("ההשאלה המבוקשת אינה קיימת");

        if (loan.MemberId != memberId)
        {
            throw new UnauthorizedAccessException("אין הרשאה להחזיר השאלה של מנוי אחר");
        }

        if (loan.Status == LoanStatus.Returned)
        {
            throw new InvalidOperationException("הספר כבר הוחזר");
        }

        loan.Status = LoanStatus.Returned;
        loan.ReturnDate = DateTime.UtcNow;
        loan.Book.IsAvailable = BookStatus.Available;

        _loanRepository.Update(loan);

        try
        {
            await _loanRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new LoanConcurrencyException("הספר עודכן על ידי משתמש אחר. נסה שוב");
        }

        return _mapper.Map<LoanResponse>(loan);
    }
}
