using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Data.Repositories;
using LibraryManagement.Services.Interfaces;
using LibraryManagement.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ILoanService, LoanService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/api/books", async (IBookService bookService, bool? available) =>
{
    var books = await bookService.GetAllBooksAsync(available);
    return Results.Ok(books);
});

app.MapGet("/api/books/{id:int}", async (IBookService bookService, int id) =>
{
    var book = await bookService.GetBookByIdAsync(id);
    return book is not null ? Results.Ok(book) : Results.NotFound();
});

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.Migrate();

    if (!db.Books.Any())
    {
        db.Books.AddRange(
            new LibraryManagement.Data.Entities.Book { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "9780743273565", TotalCopies = 5, AvailableCopies = 5 },
            new LibraryManagement.Data.Entities.Book { Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "9780061120084", TotalCopies = 3, AvailableCopies = 3 },
            new LibraryManagement.Data.Entities.Book { Title = "1984", Author = "George Orwell", ISBN = "9780451524935", TotalCopies = 4, AvailableCopies = 4 },
            new LibraryManagement.Data.Entities.Book { Title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "9780141439518", TotalCopies = 2, AvailableCopies = 2 },
            new LibraryManagement.Data.Entities.Book { Title = "The Catcher in the Rye", Author = "J.D. Salinger", ISBN = "9780316769488", TotalCopies = 3, AvailableCopies = 3 },
            new LibraryManagement.Data.Entities.Book { Title = "One Hundred Years of Solitude", Author = "Gabriel Garcia Marquez", ISBN = "9780060883287", TotalCopies = 2, AvailableCopies = 2 },
            new LibraryManagement.Data.Entities.Book { Title = "Brave New World", Author = "Aldous Huxley", ISBN = "9780060850524", TotalCopies = 3, AvailableCopies = 3 },
            new LibraryManagement.Data.Entities.Book { Title = "The Lord of the Rings", Author = "J.R.R. Tolkien", ISBN = "9780544003415", TotalCopies = 5, AvailableCopies = 5 },
            new LibraryManagement.Data.Entities.Book { Title = "Animal Farm", Author = "George Orwell", ISBN = "9780451526342", TotalCopies = 4, AvailableCopies = 4 },
            new LibraryManagement.Data.Entities.Book { Title = "Jane Eyre", Author = "Charlotte Bronte", ISBN = "9780141441146", TotalCopies = 2, AvailableCopies = 2 }
        );
        db.SaveChanges();
    }
}

app.Run();

public partial class Program { }
