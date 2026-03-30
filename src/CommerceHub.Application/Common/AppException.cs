namespace CommerceHub.Application.Common;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }
}