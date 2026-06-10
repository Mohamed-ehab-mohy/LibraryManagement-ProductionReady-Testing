namespace LibraryManagement.Services.Exceptions;

public class AlreadyReturnedException : Exception
{
    public AlreadyReturnedException() : base("This loan has already been returned.") { }
}
