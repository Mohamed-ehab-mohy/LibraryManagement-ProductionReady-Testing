using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data.Entities;

namespace LibraryManagement.Data.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        return await _context.Books.ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books.FindAsync(id);
    }

    public async Task<Book?> GetByISBNAsync(string isbn)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
    }

    public async Task<IEnumerable<Book>> GetAvailableAsync()
    {
        return await _context.Books.Where(b => b.AvailableCopies > 0).ToListAsync();
    }

    public async Task<Book> AddAsync(Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task DecrementAvailableCopiesAsync(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book != null)
        {
            book.AvailableCopies--;
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementAvailableCopiesAsync(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book != null)
        {
            book.AvailableCopies++;
            await _context.SaveChangesAsync();
        }
    }
}
