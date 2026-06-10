namespace LibraryManagement.Data.Entities;

public class Member
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime MembershipExpiryDate { get; set; }
    public decimal OutstandingFine { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
