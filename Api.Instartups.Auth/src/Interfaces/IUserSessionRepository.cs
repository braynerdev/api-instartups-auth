using Api.Instartups.Auth.Models;

namespace Api.Instartups.Auth.Interfaces;

public interface IUserSessionRepository
{
    Task LoadActiveSessionsAsync(ApplicationUser user, CancellationToken ct);

    Task<UserSessionsModel?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);

    Task AddAsync(UserSessionsModel session, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
