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
}

app.Run();

public partial class Program { }
