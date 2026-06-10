using LibraryManagement.Data.Repositories;
using LibraryManagement.Data.Entities;
using LibraryManagement.Services.Interfaces;
using LibraryManagement.Services.DTOs;
using LibraryManagement.Services.Exceptions;

namespace LibraryManagement.Services.Implementations;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;

    public LoanService(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        IMemberRepository memberRepository)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
    }

    public async Task<LoanDto> BorrowBookAsync(BorrowRequestDto dto)
    {
        var member = await _memberRepository.GetByIdAsync(dto.MemberId);
        if (member == null)
            throw new KeyNotFoundException("Member not found.");

        if (member.MembershipExpiryDate < DateTime.UtcNow)
            throw new MembershipExpiredException();

        if (member.OutstandingFine > 0)
            throw new OutstandingFineException();

        var activeLoanCount = await _loanRepository.GetActiveLoanCountAsync(dto.MemberId);
        if (activeLoanCount >= 3)
            throw new LoanLimitExceededException();

        var book = await _bookRepository.GetByIdAsync(dto.BookId);
        if (book == null)
            throw new KeyNotFoundException("Book not found.");

        if (book.AvailableCopies <= 0)
            throw new BookNotAvailableException();

        var loan = new Loan
        {
            BookId = dto.BookId,
            MemberId = dto.MemberId,
            BorrowedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        var created = await _loanRepository.AddAsync(loan);
        await _bookRepository.DecrementAvailableCopiesAsync(dto.BookId);

        created.Book = book;

        return new LoanDto
        {
            Id = created.Id,
            BookId = created.BookId,
            BookTitle = created.Book.Title,
            MemberId = created.MemberId,
            BorrowedAt = created.BorrowedAt,
            DueDate = created.DueDate,
            ReturnedAt = created.ReturnedAt,
            FineAmount = created.FineAmount
        };
    }

    public async Task<ReturnResultDto> ReturnBookAsync(int loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan == null)
            throw new KeyNotFoundException("Loan not found.");

        if (loan.ReturnedAt != null)
            throw new AlreadyReturnedException();

        loan.ReturnedAt = DateTime.UtcNow;

        if (loan.ReturnedAt > loan.DueDate)
        {
            loan.FineAmount = CalculateFine(loan.ReturnedAt.Value, loan.DueDate);

            var member = await _memberRepository.GetByIdAsync(loan.MemberId);
            if (member != null)
            {
                member.OutstandingFine += loan.FineAmount;
                await _memberRepository.UpdateAsync(member);
            }
        }

        await _loanRepository.UpdateAsync(loan);
        await _bookRepository.IncrementAvailableCopiesAsync(loan.BookId);

        return new ReturnResultDto
        {
            LoanId = loan.Id,
            ReturnedAt = loan.ReturnedAt.Value,
            FineAmount = loan.FineAmount,
            IsOverdue = loan.FineAmount > 0
        };
    }

    public async Task<IEnumerable<LoanDto>> GetMemberLoansAsync(int memberId)
    {
        var loans = await _loanRepository.GetByMemberIdAsync(memberId);

        return loans.Select(l => new LoanDto
        {
            Id = l.Id,
            BookId = l.BookId,
            BookTitle = l.Book?.Title ?? "Unknown",
            MemberId = l.MemberId,
            BorrowedAt = l.BorrowedAt,
            DueDate = l.DueDate,
            ReturnedAt = l.ReturnedAt,
            FineAmount = l.FineAmount
        });
    }

    public static decimal CalculateFine(DateTime returnedAt, DateTime dueDate)
    {
        if (returnedAt <= dueDate)
            return 0;

        var daysOverdue = (returnedAt - dueDate).Days;
        return daysOverdue * 0.50m;
    }
}
