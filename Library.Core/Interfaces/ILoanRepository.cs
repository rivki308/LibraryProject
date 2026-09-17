using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Loan?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken);
    Task<List<Loan>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken);
    Task<List<Loan>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> HasLoansAsync(int bookId, CancellationToken cancellationToken);
    Task AddAsync(Loan loan, CancellationToken cancellationToken);
    void Update(Loan loan);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
