using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.Exceptions.Base;

namespace Api.Instartups.Auth.Exceptions;

public class IdentityValidationException : ConflictException
{
    private IReadOnlyCollection<ValidateErrorDTO> Error { get; init; }
    
    public IdentityValidationException(IEnumerable<ValidateErrorDTO> errors) 
        : base("Erro de validação.")
    {
        Error = errors.ToList();
    }
}