namespace LibraryManagement.Services.Exceptions;

public class MembershipExpiredException : Exception
{
    public MembershipExpiredException() : base("Membership has expired.") { }
}
