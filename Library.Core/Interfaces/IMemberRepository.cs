using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<List<Member>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(Member member, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
