namespace Api.Instartups.Auth.src.UseCases.User.UpdateMeCommand;

public sealed record UpdateMeRequest(
        string UserName,
        string Email,
        string? PhoneNumber
    );
