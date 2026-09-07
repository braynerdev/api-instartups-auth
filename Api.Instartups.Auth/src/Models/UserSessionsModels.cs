using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Models;

public class UserSessionsModels
{
    public string TokenHash { get; private set; }

    public DateTimeOffset ExpiresAt {  get; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public Guid UserId { get; private set; }
    public IdentityUser User { get; private set; }


    private UserSessionsModels(string tokenHash, DateTimeOffset expiresAt, Guid userId)
    {
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        UserId = userId;
    }

    public static UserSessionsModels Create(string tokenHash, DateTimeOffset expiresAt, Guid userId)
    {
        return new UserSessionsModels(tokenHash, expiresAt, userId);
    }
    
}