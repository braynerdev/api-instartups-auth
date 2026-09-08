using Api.Instartups.Auth.Configurations.Database;
using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Instartups.Auth.Repositories;

public class UserSessionRepository(AppDbContext context) : IUserSessionRepository
{
    public async Task LoadActiveSessionsAsync(ApplicationUser user, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        await context.Entry(user)
            .Collection(u => u.UserSessionsModel)
            .Query()
            .Where(s => s.RevokedAt == null && s.ExpiresAt > now)
            .LoadAsync(ct);
    }

    public Task<UserSessionsModel?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
    {
        return context.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, ct);
    }

    public async Task AddAsync(UserSessionsModel session, CancellationToken ct)
    {
        context.UserSessions.Add(session);
        await context.SaveChangesAsync(ct);
    }
}
