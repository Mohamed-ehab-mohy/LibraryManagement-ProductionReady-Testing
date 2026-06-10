using LibraryManagement.Data.Entities;

namespace LibraryManagement.Data.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(int id);
    Task<Member?> GetByEmailAsync(string email);
    Task<Member> AddAsync(Member member);
    Task UpdateAsync(Member member);
}
