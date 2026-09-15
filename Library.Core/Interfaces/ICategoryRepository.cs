using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Category category, CancellationToken cancellationToken);
    void Update(Category category);
    void Delete(Category category);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}
