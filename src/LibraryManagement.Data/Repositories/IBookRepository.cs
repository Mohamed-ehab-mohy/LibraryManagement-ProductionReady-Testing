using LibraryManagement.Data.Entities;

namespace LibraryManagement.Data.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task<Book?> GetByISBNAsync(string isbn);
    Task<IEnumerable<Book>> GetAvailableAsync();
    Task<Book> AddAsync(Book book);
    Task DecrementAvailableCopiesAsync(int bookId);
    Task IncrementAvailableCopiesAsync(int bookId);
}
