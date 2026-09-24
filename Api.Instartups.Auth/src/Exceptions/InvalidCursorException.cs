using Api.Instartups.Auth.Exceptions.Base;

namespace Api.Instartups.Auth.Exceptions;

public class InvalidCursorException : BadRequestException
{
    public InvalidCursorException()
        : base("Cursor de paginação inválido.")
    {
    }
}
