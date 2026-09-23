namespace Api.Instartups.Auth.Constants;

public static class PermissionConst
{
    public sealed record Permission(string Name);

    public static readonly Permission UsersView = new("Users.View");
    public static readonly Permission UsersCreate = new("Users.Create");
    public static readonly Permission UsersUpdate = new("Users.Update");
    public static readonly Permission UsersDelete = new("Users.Delete");

    public static readonly Permission SessionsRevoke = new("Sessions.Revoke");

    public static readonly IReadOnlyList<Permission> All =
    [
        UsersView, UsersCreate, UsersUpdate, UsersDelete,
        SessionsRevoke
    ];
}
