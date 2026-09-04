namespace Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;

public sealed record RegisterUserCommandResponse(
        string Username,
        string Email,
        string Password,
        string? PhoneNumber
    );