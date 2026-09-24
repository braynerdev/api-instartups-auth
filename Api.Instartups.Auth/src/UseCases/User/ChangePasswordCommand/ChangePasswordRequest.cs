namespace Api.Instartups.Auth.src.UseCases.User.ChangePasswordCommand;

public sealed record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );
