using Api.Instartups.Auth.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Models;

public class ApplicationUser : IdentityUser
{
    public const int MaxActiveSessions = 3;

    private readonly List<UserSessionsModel> _userSessionsModel = [];
    public IReadOnlyCollection<UserSessionsModel> UserSessionsModel => _userSessionsModel;

    public UserSessionsModel AddSession(string tokenHash, DateTimeOffset expiresAt)
    {
        var now = DateTimeOffset.UtcNow;
        var activeSessions = _userSessionsModel
            .Count(s => s.RevokedAt is null && s.ExpiresAt > now);

        if (activeSessions >= MaxActiveSessions)
            throw new MaxActiveSessionsException();

        var session = Models.UserSessionsModel.Create(tokenHash, expiresAt, Id);
        _userSessionsModel.Add(session);
        return session;
    }
}
