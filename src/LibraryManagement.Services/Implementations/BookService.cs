using LibraryManagement.Data.Repositories;
using LibraryManagement.Data.Entities;
using LibraryManagement.Services.Interfaces;
using LibraryManagement.Services.DTOs;

namespace LibraryManagement.Services.Implementations;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync(bool? available = null)
    {
        IEnumerable<Book> books;

        if (available == true)
            books = await _bookRepository.GetAvailableAsync();
        else
            books = await _bookRepository.GetAllAsync();

        return books.Select(b => new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            ISBN = b.ISBN,
            TotalCopies = b.TotalCopies,
            AvailableCopies = b.AvailableCopies
        });
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return null;

        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies
        };
    }

    public async Task<BookDto> CreateBookAsync(CreateBookDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new InvalidOperationException("Title is required.");

        if (string.IsNullOrWhiteSpace(dto.Author))
            throw new InvalidOperationException("Author is required.");

        if (dto.ISBN == null || dto.ISBN.Length != 13 || !dto.ISBN.All(char.IsDigit))
            throw new InvalidOperationException("ISBN must be exactly 13 digits.");

        if (dto.TotalCopies < 1)
            throw new InvalidOperationException("TotalCopies must be at least 1.");

        var existing = await _bookRepository.GetByISBNAsync(dto.ISBN);
        if (existing != null)
            throw new InvalidOperationException("A book with this ISBN already exists.");

        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies
        };

        var created = await _bookRepository.AddAsync(book);

        return new BookDto
        {
            Id = created.Id,
            Title = created.Title,
            Author = created.Author,
            ISBN = created.ISBN,
            TotalCopies = created.TotalCopies,
            AvailableCopies = created.AvailableCopies
        };
    }
}
