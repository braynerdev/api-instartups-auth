namespace Api.Instartups.Auth.src.Interfaces.Query;

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery
{
    public Task<TResponse> Handle(TQuery query, CancellationToken ct);
}
