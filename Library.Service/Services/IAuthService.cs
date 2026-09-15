using Library.Core.DTOs.Members;

namespace Library.Service.Services;

public interface IAuthService
{
    Task<MemberResponse> RegisterAsync(MemberRegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(MemberLoginRequest request, CancellationToken cancellationToken);
}
