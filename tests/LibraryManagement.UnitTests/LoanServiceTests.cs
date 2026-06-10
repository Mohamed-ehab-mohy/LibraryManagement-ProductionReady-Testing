using Moq;
using Shouldly;
using LibraryManagement.Data.Entities;
using LibraryManagement.Data.Repositories;
using LibraryManagement.Services.DTOs;
using LibraryManagement.Services.Exceptions;
using LibraryManagement.Services.Implementations;

namespace LibraryManagement.UnitTests;

public class LoanServiceTests
{
    private readonly Mock<ILoanRepository> _loanRepoMock = new();
    private readonly Mock<IBookRepository> _bookRepoMock = new();
    private readonly Mock<IMemberRepository> _memberRepoMock = new();
    private readonly LoanService _sut;

    private readonly Member _validMember = new()
    {
        Id = 1,
        FullName = "Ahmed",
        Email = "ahmed@test.com",
        MembershipExpiryDate = DateTime.UtcNow.AddMonths(1),
        OutstandingFine = 0
    };

    private readonly Book _availableBook = new()
    {
        Id = 1,
        Title = "Clean Code",
        Author = "Robert C. Martin",
        ISBN = "9780132350884",
        TotalCopies = 5,
        AvailableCopies = 3
    };

    private readonly BorrowRequestDto _validRequest = new() { MemberId = 1, BookId = 1 };

    public LoanServiceTests()
    {
        _sut = new LoanService(_loanRepoMock.Object, _bookRepoMock.Object, _memberRepoMock.Object);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenMemberNotFound_ThrowsKeyNotFoundException()
    {
        _memberRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))!.ReturnsAsync((Member?)null);

        var ex = await Should.ThrowAsync<KeyNotFoundException>(() => _sut.BorrowBookAsync(_validRequest));

        ex.Message.ShouldBe("Member not found.");
    }

    [Fact]
    public async Task BorrowBookAsync_WhenMembershipExpired_ThrowsMembershipExpiredException()
    {
        var expiredMember = new Member
        {
            Id = 1,
            FullName = "Ahmed",
            Email = "ahmed@test.com",
            MembershipExpiryDate = DateTime.UtcNow.AddDays(-1),
            OutstandingFine = 0
        };
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expiredMember);

        await Should.ThrowAsync<MembershipExpiredException>(() => _sut.BorrowBookAsync(_validRequest));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenMemberHasOutstandingFine_ThrowsOutstandingFineException()
    {
        var memberWithFine = new Member
        {
            Id = 1,
            FullName = "Ahmed",
            Email = "ahmed@test.com",
            MembershipExpiryDate = DateTime.UtcNow.AddMonths(1),
            OutstandingFine = 10
        };
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(memberWithFine);

        await Should.ThrowAsync<OutstandingFineException>(() => _sut.BorrowBookAsync(_validRequest));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenMemberHasMaxActiveLoans_ThrowsLoanLimitExceededException()
    {
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_validMember);
        _loanRepoMock.Setup(r => r.GetActiveLoanCountAsync(1)).ReturnsAsync(3);

        await Should.ThrowAsync<LoanLimitExceededException>(() => _sut.BorrowBookAsync(_validRequest));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenBookNotFound_ThrowsKeyNotFoundException()
    {
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_validMember);
        _loanRepoMock.Setup(r => r.GetActiveLoanCountAsync(1)).ReturnsAsync(0);
        _bookRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))!.ReturnsAsync((Book?)null);

        var ex = await Should.ThrowAsync<KeyNotFoundException>(() => _sut.BorrowBookAsync(_validRequest));

        ex.Message.ShouldBe("Book not found.");
    }

    [Fact]
    public async Task BorrowBookAsync_WhenBookHasNoAvailableCopies_ThrowsBookNotAvailableException()
    {
        var unavailableBook = new Book
        {
            Id = 1,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "9780132350884",
            TotalCopies = 1,
            AvailableCopies = 0
        };

        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_validMember);
        _loanRepoMock.Setup(r => r.GetActiveLoanCountAsync(1)).ReturnsAsync(0);
        _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(unavailableBook);

        await Should.ThrowAsync<BookNotAvailableException>(() => _sut.BorrowBookAsync(_validRequest));
    }

    [Fact]
    public async Task BorrowBookAsync_Success_CreatesLoanAndDecrementsCopies()
    {
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_validMember);
        _loanRepoMock.Setup(r => r.GetActiveLoanCountAsync(1)).ReturnsAsync(0);
        _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_availableBook);
        _loanRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Loan>()))
            .ReturnsAsync((Loan loan) =>
            {
                loan.Id = 99;
                return loan;
            });

        var result = await _sut.BorrowBookAsync(_validRequest);

        result.Id.ShouldBe(99);
        result.BookId.ShouldBe(1);
        result.BookTitle.ShouldBe(_availableBook.Title);
        result.MemberId.ShouldBe(1);
        result.BorrowedAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        result.DueDate.ShouldBe(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(2));
        result.ReturnedAt.ShouldBeNull();
        result.FineAmount.ShouldBe(0);

        _bookRepoMock.Verify(r => r.DecrementAvailableCopiesAsync(1), Times.Once);
    }

    [Fact]
    public async Task BorrowBookAsync_Success_SetsDueDateTo14DaysFromBorrowed()
    {
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_validMember);
        _loanRepoMock.Setup(r => r.GetActiveLoanCountAsync(1)).ReturnsAsync(0);
        _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_availableBook);
        _loanRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Loan>()))
            .ReturnsAsync((Loan loan) =>
            {
                loan.Id = 99;
                return loan;
            });

        var result = await _sut.BorrowBookAsync(_validRequest);

        (result.DueDate - result.BorrowedAt).Days.ShouldBe(14);
    }

    [Fact]
    public async Task ReturnBookAsync_WhenLoanNotFound_ThrowsKeyNotFoundException()
    {
        _loanRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))!.ReturnsAsync((Loan?)null);

        var ex = await Should.ThrowAsync<KeyNotFoundException>(() => _sut.ReturnBookAsync(1));

        ex.Message.ShouldBe("Loan not found.");
    }

    [Fact]
    public async Task ReturnBookAsync_WhenAlreadyReturned_ThrowsAlreadyReturnedException()
    {
        var returnedLoan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            BorrowedAt = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(4),
            ReturnedAt = DateTime.UtcNow.AddDays(-5),
            FineAmount = 0
        };
        _loanRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(returnedLoan);

        await Should.ThrowAsync<AlreadyReturnedException>(() => _sut.ReturnBookAsync(1));
    }

    [Fact]
    public async Task ReturnBookAsync_WhenReturnedOnTime_SetsNoFineAndIncrementsCopies()
    {
        var activeLoan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            BorrowedAt = DateTime.UtcNow.AddDays(-5),
            DueDate = DateTime.UtcNow.AddDays(2),
            ReturnedAt = null,
            FineAmount = 0
        };

        _loanRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeLoan);

        var result = await _sut.ReturnBookAsync(1);

        result.LoanId.ShouldBe(1);
        result.ReturnedAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        result.FineAmount.ShouldBe(0);
        result.IsOverdue.ShouldBeFalse();

        _bookRepoMock.Verify(r => r.IncrementAvailableCopiesAsync(1), Times.Once);
        _loanRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Loan>()), Times.Once);
    }

    [Fact]
    public async Task ReturnBookAsync_WhenReturnedOverdue_CalculatesFineAndUpdatesMember()
    {
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var activeLoan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            BorrowedAt = DateTime.UtcNow.AddDays(-19),
            DueDate = dueDate,
            ReturnedAt = null,
            FineAmount = 0
        };

        _loanRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeLoan);
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_validMember);

        var result = await _sut.ReturnBookAsync(1);

        result.LoanId.ShouldBe(1);
        result.ReturnedAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        result.FineAmount.ShouldBe(2.50m);
        result.IsOverdue.ShouldBeTrue();

        _validMember.OutstandingFine.ShouldBe(2.50m);
        _memberRepoMock.Verify(r => r.UpdateAsync(_validMember), Times.Once);
        _bookRepoMock.Verify(r => r.IncrementAvailableCopiesAsync(1), Times.Once);
        _loanRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Loan>()), Times.Once);
    }

    [Fact]
    public async Task ReturnBookAsync_WhenReturnedOnDueDate_NoFine()
    {
        var dueDate = DateTime.UtcNow;
        var activeLoan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            BorrowedAt = DateTime.UtcNow.AddDays(-14),
            DueDate = dueDate,
            ReturnedAt = null,
            FineAmount = 0
        };

        _loanRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeLoan);

        var result = await _sut.ReturnBookAsync(1);

        result.FineAmount.ShouldBe(0);
        result.IsOverdue.ShouldBeFalse();
    }
}
