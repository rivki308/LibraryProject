using Library.Core.DTOs.Loans;

namespace Library.Service.Services;

public interface ILoanService
{
    Task<LoanResponse> CreateAsync(int memberId, LoanCreateRequest request, CancellationToken cancellationToken);
    Task<LoanResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<LoanResponse>> GetMyLoansAsync(int memberId, CancellationToken cancellationToken);
    Task<LoanResponse> ReturnAsync(int loanId, int memberId, CancellationToken cancellationToken);
}
