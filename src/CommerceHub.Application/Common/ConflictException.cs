namespace CommerceHub.Application.Common;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message)
    {
    }
}