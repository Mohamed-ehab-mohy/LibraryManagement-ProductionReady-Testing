using Moq;
using Shouldly;
using LibraryManagement.Data.Entities;
using LibraryManagement.Data.Repositories;
using LibraryManagement.Services.DTOs;
using LibraryManagement.Services.Exceptions;
using LibraryManagement.Services.Implementations;

namespace LibraryManagement.UnitTests;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepoMock = new();
    private readonly BookService _sut;

    public BookServiceTests()
    {
        _sut = new BookService(_bookRepoMock.Object);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenNoFilter_ReturnsAllBooks()
    {
        var books = new List<Book>
        {
            new() { Id = 1, Title = "A", Author = "X", ISBN = "1111111111111", TotalCopies = 2, AvailableCopies = 1 },
            new() { Id = 2, Title = "B", Author = "Y", ISBN = "2222222222222", TotalCopies = 1, AvailableCopies = 0 }
        };
        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        var result = await _sut.GetAllBooksAsync();

        result.Count().ShouldBe(2);
        _bookRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
        _bookRepoMock.Verify(r => r.GetAvailableAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenAvailableTrue_ReturnsAvailableBooks()
    {
        var available = new List<Book>
        {
            new() { Id = 1, Title = "A", Author = "X", ISBN = "1111111111111", TotalCopies = 2, AvailableCopies = 2 }
        };
        _bookRepoMock.Setup(r => r.GetAvailableAsync()).ReturnsAsync(available);

        var result = await _sut.GetAllBooksAsync(true);

        result.Count().ShouldBe(1);
        _bookRepoMock.Verify(r => r.GetAvailableAsync(), Times.Once);
        _bookRepoMock.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenAvailableFalse_ReturnsAllBooks()
    {
        var books = new List<Book>
        {
            new() { Id = 1, Title = "A", Author = "X", ISBN = "1111111111111", TotalCopies = 1, AvailableCopies = 0 }
        };
        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        var result = await _sut.GetAllBooksAsync(false);

        result.Count().ShouldBe(1);
        _bookRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllBooksAsync_MapsToDtoCorrectly()
    {
        var books = new List<Book>
        {
            new() { Id = 1, Title = "Clean Code", Author = "Martin", ISBN = "9780132350884", TotalCopies = 5, AvailableCopies = 3 }
        };
        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        var result = await _sut.GetAllBooksAsync();

        var dto = result.Single();
        dto.Id.ShouldBe(1);
        dto.Title.ShouldBe("Clean Code");
        dto.Author.ShouldBe("Martin");
        dto.ISBN.ShouldBe("9780132350884");
        dto.TotalCopies.ShouldBe(5);
        dto.AvailableCopies.ShouldBe(3);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenExists_ReturnsBook()
    {
        var book = new Book { Id = 1, Title = "Clean Code", Author = "Martin", ISBN = "9780132350884", TotalCopies = 5, AvailableCopies = 3 };
        _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

        var result = await _sut.GetBookByIdAsync(1);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.Title.ShouldBe("Clean Code");
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenNotExists_ReturnsNull()
    {
        _bookRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))!.ReturnsAsync((Book?)null);

        var result = await _sut.GetBookByIdAsync(999);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateBookAsync_WhenTitleIsEmpty_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "", Author = "Martin", ISBN = "9780132350884", TotalCopies = 1 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("Title is required.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenTitleIsWhitespace_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "   ", Author = "Martin", ISBN = "9780132350884", TotalCopies = 1 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("Title is required.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenAuthorIsEmpty_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "", ISBN = "9780132350884", TotalCopies = 1 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("Author is required.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenAuthorIsWhitespace_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "   ", ISBN = "9780132350884", TotalCopies = 1 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("Author is required.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenIsbnIsNull_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "Martin", ISBN = null!, TotalCopies = 1 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("ISBN must be exactly 13 digits.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenIsbnIsLessThan13Digits_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "Martin", ISBN = "123", TotalCopies = 1 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("ISBN must be exactly 13 digits.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenIsbnIsMoreThan13Digits_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "Martin", ISBN = "12345678901234", TotalCopies = 1 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("ISBN must be exactly 13 digits.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenIsbnContainsNonDigits_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "Martin", ISBN = "978013235088X", TotalCopies = 1 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("ISBN must be exactly 13 digits.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenTotalCopiesIsZero_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "Martin", ISBN = "9780132350884", TotalCopies = 0 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("TotalCopies must be at least 1.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenTotalCopiesIsNegative_ThrowsInvalidOperationException()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "Martin", ISBN = "9780132350884", TotalCopies = -1 };

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _sut.CreateBookAsync(dto));

        ex.Message.ShouldBe("TotalCopies must be at least 1.");
    }

    [Fact]
    public async Task CreateBookAsync_WhenIsbnAlreadyExists_ThrowsDuplicateIsbnException()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "Martin", ISBN = "9780132350884", TotalCopies = 1 };
        _bookRepoMock.Setup(r => r.GetByISBNAsync("9780132350884")).ReturnsAsync(new Book());

        await Should.ThrowAsync<DuplicateIsbnException>(() => _sut.CreateBookAsync(dto));
    }

    [Fact]
    public async Task CreateBookAsync_Success_CreatesAndReturnsBook()
    {
        var dto = new CreateBookDto { Title = "Clean Code", Author = "Martin", ISBN = "9780132350884", TotalCopies = 3 };
        _bookRepoMock.Setup(r => r.GetByISBNAsync("9780132350884"))!.ReturnsAsync((Book?)null);
        _bookRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Book>()))
            .ReturnsAsync((Book b) =>
            {
                b.Id = 10;
                return b;
            });

        var result = await _sut.CreateBookAsync(dto);

        result.Id.ShouldBe(10);
        result.Title.ShouldBe("Clean Code");
        result.Author.ShouldBe("Martin");
        result.ISBN.ShouldBe("9780132350884");
        result.TotalCopies.ShouldBe(3);
        result.AvailableCopies.ShouldBe(3);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public async Task GetAllBooksAsync_WithVariousFilters_DelegatesToCorrectMethod(bool? available)
    {
        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Book>());
        _bookRepoMock.Setup(r => r.GetAvailableAsync()).ReturnsAsync(new List<Book>());

        await _sut.GetAllBooksAsync(available);

        if (available == true)
            _bookRepoMock.Verify(r => r.GetAvailableAsync(), Times.Once);
        else
            _bookRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
