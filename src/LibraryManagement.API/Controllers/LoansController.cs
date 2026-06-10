using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Services.Interfaces;
using LibraryManagement.Services.DTOs;
using LibraryManagement.Services.Exceptions;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost]
    public async Task<IActionResult> BorrowBook([FromBody] BorrowRequestDto dto)
    {
        try
        {
            var loan = await _loanService.BorrowBookAsync(dto);
            return Created($"/api/loans/{loan.Id}", loan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (MembershipExpiredException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (OutstandingFineException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (LoanLimitExceededException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (BookNotAvailableException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/return")]
    public async Task<IActionResult> ReturnBook(int id)
    {
        try
        {
            var result = await _loanService.ReturnBookAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (AlreadyReturnedException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMemberLoans([FromQuery] int memberId)
    {
        var loans = await _loanService.GetMemberLoansAsync(memberId);
        return Ok(loans);
    }
}
