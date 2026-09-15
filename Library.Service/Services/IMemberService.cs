using Library.Core.DTOs.Members;

namespace Library.Service.Services;

public interface IMemberService
{
    Task<MemberResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<MemberResponse>> GetAllAsync(CancellationToken cancellationToken);
}
