using Library.Core.DTOs.Categories;

namespace Library.Service.Services;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken cancellationToken);
}
