using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using LibraryManagement.Data;
using LibraryManagement.Data.Entities;
using LibraryManagement.Data.Repositories;

namespace LibraryManagement.UnitTests;

public class MemberRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LibraryDbContext _context;
    private readonly MemberRepository _repo;

    public MemberRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LibraryDbContext(options);
        _context.Database.EnsureCreated();

        _repo = new MemberRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsMember()
    {
        var member = new Member { FullName = "Ahmed", Email = "ahmed@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(1) };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(member.Id);

        result.ShouldNotBeNull();
        result.FullName.ShouldBe("Ahmed");
        result.Email.ShouldBe("ahmed@test.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(999);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WhenExists_ReturnsMember()
    {
        _context.Members.Add(new Member { FullName = "Ali", Email = "ali@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(1) });
        await _context.SaveChangesAsync();

        var result = await _repo.GetByEmailAsync("ali@test.com");

        result.ShouldNotBeNull();
        result.FullName.ShouldBe("Ali");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenNotExists_ReturnsNull()
    {
        var result = await _repo.GetByEmailAsync("nobody@test.com");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsync_AddsMemberAndSetsId()
    {
        var member = new Member { FullName = "Omar", Email = "omar@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(1) };

        var result = await _repo.AddAsync(member);

        result.Id.ShouldBeGreaterThan(0);
        result.Email.ShouldBe("omar@test.com");
        result.OutstandingFine.ShouldBe(0);
    }

    [Fact]
    public async Task AddAsync_WhenDuplicateEmail_ThrowsDbUpdateException()
    {
        _context.Members.Add(new Member { FullName = "First", Email = "dup@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(1) });
        await _context.SaveChangesAsync();

        var duplicate = new Member { FullName = "Second", Email = "dup@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(1) };

        await Should.ThrowAsync<DbUpdateException>(() => _repo.AddAsync(duplicate));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesMember()
    {
        var member = new Member { FullName = "Khaled", Email = "khaled@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(1) };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        member.FullName = "Khaled Updated";
        member.OutstandingFine = 5.50m;

        await _repo.UpdateAsync(member);

        var reloaded = await _context.Members.FindAsync(member.Id);
        reloaded!.FullName.ShouldBe("Khaled Updated");
        reloaded.OutstandingFine.ShouldBe(5.50m);
    }

    [Fact]
    public async Task OutstandingFine_HasDefaultValue_Zero()
    {
        var member = new Member { FullName = "Noor", Email = "noor@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(1) };

        await _repo.AddAsync(member);

        member.OutstandingFine.ShouldBe(0);
    }
}
