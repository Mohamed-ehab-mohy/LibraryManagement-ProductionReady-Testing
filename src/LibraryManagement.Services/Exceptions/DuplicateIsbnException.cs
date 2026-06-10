namespace LibraryManagement.Services.Exceptions;

public class DuplicateIsbnException : Exception
{
    public DuplicateIsbnException() : base("A book with this ISBN already exists.") { }
}
