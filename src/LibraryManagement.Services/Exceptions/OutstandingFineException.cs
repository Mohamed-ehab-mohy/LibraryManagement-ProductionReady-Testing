namespace LibraryManagement.Services.Exceptions;

public class OutstandingFineException : Exception
{
    public OutstandingFineException() : base("Member has an outstanding fine and cannot borrow.") { }
}
