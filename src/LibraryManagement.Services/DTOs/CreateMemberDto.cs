namespace LibraryManagement.Services.DTOs;

public class CreateMemberDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime MembershipExpiryDate { get; set; }
}
