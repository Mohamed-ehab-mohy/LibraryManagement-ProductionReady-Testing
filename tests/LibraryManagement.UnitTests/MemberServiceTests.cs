using Moq;
using Shouldly;
using LibraryManagement.Data.Entities;
using LibraryManagement.Data.Repositories;
using LibraryManagement.Services.DTOs;
using LibraryManagement.Services.Implementations;

namespace LibraryManagement.UnitTests;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _memberRepoMock = new();
    private readonly Mock<ILoanRepository> _loanRepoMock = new();
    private readonly MemberService _sut;

    public MemberServiceTests()
    {
        _sut = new MemberService(_memberRepoMock.Object, _loanRepoMock.Object);
    }

    [Fact]
    public async Task GetMemberByIdAsync_WhenExists_ReturnsMemberWithActiveLoanCount()
    {
        var member = new Member
        {
            Id = 1,
            FullName = "Ahmed",
            Email = "ahmed@test.com",
            MembershipExpiryDate = new DateTime(2027, 6, 1),
            OutstandingFine = 5.50m
        };
        _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _loanRepoMock.Setup(r => r.GetActiveLoanCountAsync(1)).ReturnsAsync(2);

        var result = await _sut.GetMemberByIdAsync(1);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.FullName.ShouldBe("Ahmed");
        result.Email.ShouldBe("ahmed@test.com");
        result.MembershipExpiryDate.ShouldBe(new DateTime(2027, 6, 1));
        result.OutstandingFine.ShouldBe(5.50m);
        result.ActiveLoanCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetMemberByIdAsync_WhenNotExists_ReturnsNull()
    {
        _memberRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))!.ReturnsAsync((Member?)null);

        var result = await _sut.GetMemberByIdAsync(999);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateMemberAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        var dto = new CreateMemberDto { FullName = "Ahmed", Email = "dupe@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(1) };
        _memberRepoMock.Setup(r => r.GetByEmailAsync("dupe@test.com")).ReturnsAsync(new Member());

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateMemberAsync(dto));

        ex.Message.ShouldBe("A member with this email already exists.");
    }

    [Fact]
    public async Task CreateMemberAsync_Success_CreatesAndReturnsMember()
    {
        var dto = new CreateMemberDto
        {
            FullName = "Ahmed",
            Email = "ahmed@test.com",
            MembershipExpiryDate = new DateTime(2027, 6, 1)
        };
        _memberRepoMock.Setup(r => r.GetByEmailAsync("ahmed@test.com"))!.ReturnsAsync((Member?)null);
        _memberRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Member>()))
            .ReturnsAsync((Member m) =>
            {
                m.Id = 5;
                return m;
            });

        var result = await _sut.CreateMemberAsync(dto);

        result.Id.ShouldBe(5);
        result.FullName.ShouldBe("Ahmed");
        result.Email.ShouldBe("ahmed@test.com");
        result.MembershipExpiryDate.ShouldBe(new DateTime(2027, 6, 1));
        result.OutstandingFine.ShouldBe(0);
        result.ActiveLoanCount.ShouldBe(0);
    }
}
