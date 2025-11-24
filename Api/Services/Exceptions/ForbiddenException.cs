namespace Api.Services.Exceptions;

public class ForbiddenException : Exception
{
    //также как и конфликт эксепшн
    public ForbiddenException(string message) : base(message)
    {
    }
}

