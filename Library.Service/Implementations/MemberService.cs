using AutoMapper;
using Library.Core.DTOs.Members;
using Library.Core.Interfaces;
using Library.Service.Services;

namespace Library.Service.Implementations;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMapper _mapper;

    public MemberService(IMemberRepository memberRepository, IMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<MemberResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(id, cancellationToken);
        return member is null ? null : _mapper.Map<MemberResponse>(member);
    }

    public async Task<List<MemberResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var members = await _memberRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<MemberResponse>>(members);
    }
}
