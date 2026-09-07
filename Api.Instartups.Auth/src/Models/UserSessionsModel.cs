using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Models;

public class UserSessionsModel
{
    public string Id { get; private set; }
    public string TokenHash { get; private set; }

    public DateTimeOffset ExpiresAt {  get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public string? ReplacedByTokenId { get; private set; }

    public string UserId { get; private set; }
    public IdentityUser User { get; private set; }


    private UserSessionsModel(string tokenHash, DateTimeOffset expiresAt, string userId)
    {
        Id = Guid.CreateVersion7().ToString();
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        UserId = userId;
    }

    public static UserSessionsModel Create(string tokenHash, DateTimeOffset expiresAt, string userId)
    {
        return new UserSessionsModel(tokenHash, expiresAt, userId);
    }
    
}