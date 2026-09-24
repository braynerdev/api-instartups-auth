namespace Api.Instartups.Auth.Constants;

public static class PermissionConst
{
    public sealed record Permission(string Name);

    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersUpdate = "Users.Update";
    public const string UsersDelete = "Users.Delete";

    public const string SessionsRevoke = "Sessions.Revoke";

    public const string Admin = "Admin";

    public static readonly IReadOnlyList<Permission> All =
    [
        new Permission(UsersView), new Permission(UsersCreate), new Permission(UsersUpdate), new Permission(UsersDelete),
        new Permission(SessionsRevoke),
        new Permission(Admin)
    ];
}
