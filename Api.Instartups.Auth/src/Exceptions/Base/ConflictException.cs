namespace Api.Instartups.Auth.Exceptions.Base;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) {}
}