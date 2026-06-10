using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data.Entities;

namespace LibraryManagement.Data.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly LibraryDbContext _context;

    public LoanRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Loan>> GetByMemberIdAsync(int memberId)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.BorrowedAt)
            .ToListAsync();
    }

    public async Task<int> GetActiveLoanCountAsync(int memberId)
    {
        return await _context.Loans
            .CountAsync(l => l.MemberId == memberId && l.ReturnedAt == null);
    }

    public async Task<Loan> AddAsync(Loan loan)
    {
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task UpdateAsync(Loan loan)
    {
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();
    }
}
