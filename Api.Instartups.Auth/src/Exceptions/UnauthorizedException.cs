namespace Api.Instartups.Auth.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("Token inválido.")
    {
        
    }
}