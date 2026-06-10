using Microsoft.EntityFrameworkCore;
using Shouldly;
using LibraryManagement.Data;
using LibraryManagement.Data.Entities;
using LibraryManagement.Data.Repositories;

namespace LibraryManagement.UnitTests;

public class BookRepositoryTests : IDisposable
{
    private readonly LibraryDbContext _context;
    private readonly BookRepository _repo;

    public BookRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase($"BookRepoTest_{Guid.NewGuid()}")
            .Options;

        _context = new LibraryDbContext(options);
        _repo = new BookRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllBooks()
    {
        _context.Books.AddRange(
            new Book { Title = "A", Author = "X", ISBN = "1111111111111", TotalCopies = 1, AvailableCopies = 1 },
            new Book { Title = "B", Author = "Y", ISBN = "2222222222222", TotalCopies = 2, AvailableCopies = 2 }
        );
        await _context.SaveChangesAsync();

        var books = await _repo.GetAllAsync();

        books.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsBook()
    {
        var book = new Book { Title = "Clean Code", Author = "Martin", ISBN = "9780132350884", TotalCopies = 3, AvailableCopies = 3 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(book.Id);

        result.ShouldNotBeNull();
        result.Title.ShouldBe("Clean Code");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(999);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByISBNAsync_WhenExists_ReturnsBook()
    {
        _context.Books.Add(new Book { Title = "Clean Code", Author = "Martin", ISBN = "9780132350884", TotalCopies = 3, AvailableCopies = 3 });
        await _context.SaveChangesAsync();

        var result = await _repo.GetByISBNAsync("9780132350884");

        result.ShouldNotBeNull();
        result.Title.ShouldBe("Clean Code");
    }

    [Fact]
    public async Task GetByISBNAsync_WhenNotExists_ReturnsNull()
    {
        var result = await _repo.GetByISBNAsync("0000000000000");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAvailableAsync_ReturnsOnlyBooksWithAvailableCopies()
    {
        _context.Books.AddRange(
            new Book { Title = "A", Author = "X", ISBN = "1111111111111", TotalCopies = 1, AvailableCopies = 1 },
            new Book { Title = "B", Author = "Y", ISBN = "2222222222222", TotalCopies = 2, AvailableCopies = 0 },
            new Book { Title = "C", Author = "Z", ISBN = "3333333333333", TotalCopies = 0, AvailableCopies = 0 }
        );
        await _context.SaveChangesAsync();

        var books = await _repo.GetAvailableAsync();

        books.Count().ShouldBe(1);
        books.Single().Title.ShouldBe("A");
    }

    [Fact]
    public async Task AddAsync_AddsBookAndSetsId()
    {
        var book = new Book { Title = "New Book", Author = "Me", ISBN = "9999999999999", TotalCopies = 2, AvailableCopies = 2 };

        var result = await _repo.AddAsync(book);

        result.Id.ShouldBeGreaterThan(0);
        _context.Books.Count().ShouldBe(1);
    }

    [Fact]
    public async Task DecrementAvailableCopiesAsync_DecrementsByOne()
    {
        var book = new Book { Title = "Test", Author = "T", ISBN = "1234567890123", TotalCopies = 5, AvailableCopies = 5 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        await _repo.DecrementAvailableCopiesAsync(book.Id);

        var reloaded = await _context.Books.FindAsync(book.Id);
        reloaded!.AvailableCopies.ShouldBe(4);
    }

    [Fact]
    public async Task DecrementAvailableCopiesAsync_WhenBookNotFound_DoesNothing()
    {
        await _repo.DecrementAvailableCopiesAsync(999);

        _context.Books.Count().ShouldBe(0);
    }

    [Fact]
    public async Task IncrementAvailableCopiesAsync_IncrementsByOne()
    {
        var book = new Book { Title = "Test", Author = "T", ISBN = "1234567890123", TotalCopies = 5, AvailableCopies = 2 };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        await _repo.IncrementAvailableCopiesAsync(book.Id);

        var reloaded = await _context.Books.FindAsync(book.Id);
        reloaded!.AvailableCopies.ShouldBe(3);
    }

    [Fact]
    public async Task IncrementAvailableCopiesAsync_WhenBookNotFound_DoesNothing()
    {
        await _repo.IncrementAvailableCopiesAsync(999);

        _context.Books.Count().ShouldBe(0);
    }
}
