using LibraryManagement.Data.Repositories;
using LibraryManagement.Data.Entities;
using LibraryManagement.Services.Interfaces;
using LibraryManagement.Services.DTOs;

namespace LibraryManagement.Services.Implementations;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly ILoanRepository _loanRepository;

    public MemberService(IMemberRepository memberRepository, ILoanRepository loanRepository)
    {
        _memberRepository = memberRepository;
        _loanRepository = loanRepository;
    }

    public async Task<MemberDto?> GetMemberByIdAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member == null) return null;

        var activeLoanCount = await _loanRepository.GetActiveLoanCountAsync(id);

        return new MemberDto
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            MembershipExpiryDate = member.MembershipExpiryDate,
            OutstandingFine = member.OutstandingFine,
            ActiveLoanCount = activeLoanCount
        };
    }

    public async Task<MemberDto> CreateMemberAsync(CreateMemberDto dto)
    {
        var existing = await _memberRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("A member with this email already exists.");

        var member = new Member
        {
            FullName = dto.FullName,
            Email = dto.Email,
            MembershipExpiryDate = dto.MembershipExpiryDate,
            OutstandingFine = 0
        };

        var created = await _memberRepository.AddAsync(member);

        return new MemberDto
        {
            Id = created.Id,
            FullName = created.FullName,
            Email = created.Email,
            MembershipExpiryDate = created.MembershipExpiryDate,
            OutstandingFine = created.OutstandingFine,
            ActiveLoanCount = 0
        };
    }
}
