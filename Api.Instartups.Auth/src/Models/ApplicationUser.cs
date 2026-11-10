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
            .Where(s => s.IsActive(now))
            .OrderBy(s => s.ExpiresAt)
            .ToList();

        // No limite, encerra as sessões mais antigas para abrir espaço para a nova.
        // Recusar o login deixava o usuário sem acesso por até 30 dias quando ele
        // fechava o navegador sem sair (o logout exige uma sessão válida).
        var excedentes = activeSessions.Count - MaxActiveSessions + 1;
        foreach (var sessaoAntiga in activeSessions.Take(Math.Max(excedentes, 0)))
            sessaoAntiga.Revoke(now);

        var session = Models.UserSessionsModel.Create(tokenHash, expiresAt, Id);
        _userSessionsModel.Add(session);
        return session;
    }

    public UserSessionsModel RotateSession(
        UserSessionsModel currentSession, string newTokenHash, DateTimeOffset expiresAt)
    {
        var now = DateTimeOffset.UtcNow;

        if (!currentSession.IsActive(now))
            throw new InvalidRefreshTokenException();

        var newSession = Models.UserSessionsModel.Create(newTokenHash, expiresAt, Id);
        currentSession.Revoke(now, newSession.Id);
        _userSessionsModel.Add(newSession);
        return newSession;
    }

    public void RevokeSession(UserSessionsModel currentSession)
    {
        var now = DateTimeOffset.UtcNow;

        if (!currentSession.IsActive(now))
            throw new InvalidRefreshTokenException();

        currentSession.Revoke(now);
    }

    public void RevokeAllSessions()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var session in _userSessionsModel.Where(s => s.IsActive(now)))
            session.Revoke(now);
    }
}
