using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Instartups.Auth.ModelsMaps;

public class UserSessionsModelMap : IEntityTypeConfiguration<UserSessionsModel>, IAppConfiguration
{
    public void Configure(EntityTypeBuilder<UserSessionsModel> builder)
    {
        builder.ToTable("UserSessions");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar(255)");

        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz");
        
        builder.Property(x => x.ReplacedByTokenId)
            .HasColumnName("replaced_by_token_id")
            .HasColumnType("varchar(450)");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("varchar(450)");

        builder.HasOne<UserSessionsModel>()
            .WithOne()
            .HasForeignKey<UserSessionsModel>(us => us.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ReplacedByTokenId).IsUnique();

        builder.HasOne(us => us.User)
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasIndex(x => new { x.TokenHash, x.UserId }).IsUnique();
    }
}