using LibraryManagement.Services.DTOs;

namespace LibraryManagement.Services.Interfaces;

public interface IMemberService
{
    Task<MemberDto?> GetMemberByIdAsync(int id);
    Task<MemberDto> CreateMemberAsync(CreateMemberDto dto);
}
