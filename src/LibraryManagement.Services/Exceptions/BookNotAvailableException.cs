namespace LibraryManagement.Services.Exceptions;

public class BookNotAvailableException : Exception
{
    public BookNotAvailableException() : base("The requested book is not available for borrowing.") { }
}
