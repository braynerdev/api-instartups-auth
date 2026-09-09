namespace Api.Instartups.Auth.Exceptions;

public class InvalidRefreshTokenException : UnauthorizedException
{
    public InvalidRefreshTokenException()
        : base("Refresh token inválido.")
    {
    }
}
