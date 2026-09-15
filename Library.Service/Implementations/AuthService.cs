using AutoMapper;
using Library.Core.DTOs.Members;
using Library.Core.Entities;
using Library.Core.Interfaces;
using Library.Service.Security;
using Library.Service.Services;

namespace Library.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IMemberRepository _memberRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IMapper _mapper;

    public AuthService(IMemberRepository memberRepository, ITokenGenerator tokenGenerator, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _tokenGenerator = tokenGenerator;
        _mapper = mapper;
    }

    public async Task<MemberResponse> RegisterAsync(MemberRegisterRequest request, CancellationToken cancellationToken)
    {
        if (await _memberRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("קיים כבר מנוי עם כתובת אימייל זו");
        }

        var member = new Member
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            IsAdmin = false
        };

        await _memberRepository.AddAsync(member, cancellationToken);
        await _memberRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MemberResponse>(member);
    }

    public async Task<AuthResponse> LoginAsync(MemberLoginRequest request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (member is null || !PasswordHasher.Verify(request.Password, member.PasswordHash))
        {
            throw new AuthenticationFailedException("אימייל או סיסמה שגויים");
        }

        var (token, expiresAtUtc) = _tokenGenerator.GenerateToken(member);

        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Member = _mapper.Map<MemberResponse>(member)
        };
    }
}
