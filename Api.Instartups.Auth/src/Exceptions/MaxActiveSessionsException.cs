using Api.Instartups.Auth.Exceptions.Base;

namespace Api.Instartups.Auth.Exceptions;

public class MaxActiveSessionsException : BadRequestException
{
    public MaxActiveSessionsException()
        : base("Limite de sessões ativas atingido.")
    {
    }
}
