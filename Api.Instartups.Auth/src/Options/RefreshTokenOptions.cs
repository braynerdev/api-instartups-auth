namespace Api.Instartups.Auth.Options;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int ExpirationDays { get; set; }
}
