namespace Api.Instartups.Auth.src.Interfaces.Command;

public interface ICommandHandler<T> where T : ICommand
{
    public Task<T> Handle(T Command);
}
