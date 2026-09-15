using Library.Core.Entities;

namespace Library.Service.Security;

public interface ITokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(Member member);
}
