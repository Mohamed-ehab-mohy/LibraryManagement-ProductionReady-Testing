using LibraryManagement.Data.Entities;

namespace LibraryManagement.Data.Repositories;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(int id);
    Task<IEnumerable<Loan>> GetByMemberIdAsync(int memberId);
    Task<int> GetActiveLoanCountAsync(int memberId);
    Task<Loan> AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
}
