using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
{
    public async Task<RegisterUserCommand> Handle(RegisterUserCommand Command) => throw new NotImplementedException();
}
