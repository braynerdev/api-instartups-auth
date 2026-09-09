namespace Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;

public sealed record RegisterUserCommandResponse(
        string UserName,
        string Email,
        string? PhoneNumber
    );