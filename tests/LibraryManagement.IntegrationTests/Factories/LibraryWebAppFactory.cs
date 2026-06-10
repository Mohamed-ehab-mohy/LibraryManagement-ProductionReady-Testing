using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using LibraryManagement.Data;
using LibraryManagement.Data.Entities;

namespace LibraryManagement.IntegrationTests.Factories;

public class LibraryWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = _container.GetConnectionString();

        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LibraryDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<LibraryDbContext>(options =>
                options.UseSqlServer(connectionString));
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await MigrateAndSeedAsync();
    }

    private async Task MigrateAndSeedAsync()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        using var db = new LibraryDbContext(options);
        await db.Database.MigrateAsync();

        if (!await db.Books.AnyAsync())
        {
            db.Books.AddRange(
                new Book { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780132350884", TotalCopies = 5, AvailableCopies = 5 },
                new Book { Title = "Clean Architecture", Author = "Robert C. Martin", ISBN = "9780134494166", TotalCopies = 3, AvailableCopies = 3 },
                new Book { Title = "The Pragmatic Programmer", Author = "David Thomas", ISBN = "9780135957059", TotalCopies = 4, AvailableCopies = 4 }
            );
            db.Members.AddRange(
                new Member { FullName = "Ahmed Ali", Email = "ahmed@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(6), OutstandingFine = 0 },
                new Member { FullName = "Sara Hassan", Email = "sara@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(3), OutstandingFine = 0 }
            );
            await db.SaveChangesAsync();
        }
    }

    public async Task ResetDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        using var db = new LibraryDbContext(options);
        db.Loans.RemoveRange(db.Loans);
        db.Books.RemoveRange(db.Books);
        db.Members.RemoveRange(db.Members);
        await db.SaveChangesAsync();

        db.Books.AddRange(
            new Book { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780132350884", TotalCopies = 5, AvailableCopies = 5 },
            new Book { Title = "Clean Architecture", Author = "Robert C. Martin", ISBN = "9780134494166", TotalCopies = 3, AvailableCopies = 3 },
            new Book { Title = "The Pragmatic Programmer", Author = "David Thomas", ISBN = "9780135957059", TotalCopies = 4, AvailableCopies = 4 }
        );
        db.Members.AddRange(
            new Member { FullName = "Ahmed Ali", Email = "ahmed@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(6), OutstandingFine = 0 },
            new Member { FullName = "Sara Hassan", Email = "sara@test.com", MembershipExpiryDate = DateTime.UtcNow.AddMonths(3), OutstandingFine = 0 }
        );
        await db.SaveChangesAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
