using Api.Instartups.Auth.Exceptions.Base;

namespace Api.Instartups.Auth.Exceptions;

public class InvalidCredentialsException : BadRequestException
{
    public  InvalidCredentialsException()
        : base("Credenciais inválidas.")
    {
    }
}