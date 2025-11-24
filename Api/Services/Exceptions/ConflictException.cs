namespace Api.Services.Exceptions;

public class ConflictException : Exception
{
    //то для того чтобы кидать ошибку конфликта а не генерал, чтобы можно было ловить ошибку конкретно
    // например catch(ConflictException ex) { //обработка конфликта }
    // catch(Exception ex) { //общая обработка ошибок }
    // таким образом можно разделить обработку ошибок в зависимости от их типа
    public ConflictException(string message) : base(message)
    {
    }
}

