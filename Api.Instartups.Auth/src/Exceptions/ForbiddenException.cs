namespace Api.Instartups.Auth.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException()  : base("Acesso negado.")
    {
    }

    public ForbiddenException(string message) : base(message)
    {
    }
}