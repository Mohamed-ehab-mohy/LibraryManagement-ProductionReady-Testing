using Shouldly;
using LibraryManagement.Services.Implementations;

namespace LibraryManagement.ScratchTests;

public class FluentVsShouldlyTests
{
    [Theory]
    [InlineData(0, 1, 14)]
    [InlineData(0.50, 15, 14)]
    [InlineData(1.50, 17, 14)]
    [InlineData(7.00, 28, 14)]
    [InlineData(0, 10, 14)]
    public void CalculateFine_WithShouldly(decimal expected, int returnedDay, int dueDay)
    {
        var returnedAt = new DateTime(2026, 6, returnedDay);
        var dueDate = new DateTime(2026, 6, dueDay);

        var result = LoanService.CalculateFine(returnedAt, dueDate);

        result.ShouldBe(expected);
    }
}
