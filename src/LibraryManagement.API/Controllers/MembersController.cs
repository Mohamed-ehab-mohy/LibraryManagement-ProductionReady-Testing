using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Services.Interfaces;
using LibraryManagement.Services.DTOs;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMemberById(int id)
    {
        var member = await _memberService.GetMemberByIdAsync(id);
        return member is not null ? Ok(member) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberDto dto)
    {
        try
        {
            var member = await _memberService.CreateMemberAsync(dto);
            return CreatedAtAction(nameof(GetMemberById), new { id = member.Id }, member);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
