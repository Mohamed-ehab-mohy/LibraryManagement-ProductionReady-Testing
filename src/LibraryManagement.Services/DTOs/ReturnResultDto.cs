namespace LibraryManagement.Services.DTOs;

public class ReturnResultDto
{
    public int LoanId { get; set; }
    public DateTime ReturnedAt { get; set; }
    public decimal FineAmount { get; set; }
    public bool IsOverdue { get; set; }
}
