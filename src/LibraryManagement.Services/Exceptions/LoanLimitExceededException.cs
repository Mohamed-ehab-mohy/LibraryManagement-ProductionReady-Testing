namespace LibraryManagement.Services.Exceptions;

public class LoanLimitExceededException : Exception
{
    public LoanLimitExceededException() : base("Member has reached the maximum number of active loans.") { }
}
