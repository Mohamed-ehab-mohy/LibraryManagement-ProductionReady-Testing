namespace LibraryManagement.Data.Entities;

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime BorrowedAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public decimal FineAmount { get; set; }
    public Book Book { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
