using System.Reflection;
using LibraryManagement.API.Controllers;
using LibraryManagement.Data;
using LibraryManagement.Services.Interfaces;
using NetArchTest.Rules;
using Shouldly;

namespace LibraryManagement.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly ApiAssembly = typeof(BooksController).Assembly;
    private static readonly Assembly ServicesAssembly = typeof(IBookService).Assembly;
    private static readonly Assembly DataAssembly = typeof(LibraryDbContext).Assembly;

    [Fact]
    public void Services_should_not_depend_on_API()
    {
        var result = Types
            .InAssembly(ServicesAssembly)
            .Should()
            .NotHaveDependencyOn("LibraryManagement.API")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Data_should_not_depend_on_Services_or_API()
    {
        var result = Types
            .InAssembly(DataAssembly)
            .Should()
            .NotHaveDependencyOn("LibraryManagement.Services")
            .And()
            .NotHaveDependencyOn("LibraryManagement.API")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Controllers_should_not_directly_reference_LibraryDbContext()
    {
        var result = Types
            .InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("LibraryManagement.API.Controllers")
            .Should()
            .NotHaveDependencyOn("LibraryManagement.Data.LibraryDbContext")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
