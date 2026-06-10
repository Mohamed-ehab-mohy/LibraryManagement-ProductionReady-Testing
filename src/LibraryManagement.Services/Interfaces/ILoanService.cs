using LibraryManagement.Services.DTOs;

namespace LibraryManagement.Services.Interfaces;

public interface ILoanService
{
    Task<LoanDto> BorrowBookAsync(BorrowRequestDto dto);
    Task<ReturnResultDto> ReturnBookAsync(int loanId);
    Task<IEnumerable<LoanDto>> GetMemberLoansAsync(int memberId);
}
