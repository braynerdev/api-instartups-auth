using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Instartups.Auth.Migrations
{
    /// <inheritdoc />
    public partial class UserSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSessions",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    token_hash = table.Column<string>(type: "varchar(255)", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    replaced_by_token_id = table.Column<string>(type: "varchar(450)", nullable: true),
                    user_id = table.Column<string>(type: "varchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSessions_UserSessions_replaced_by_token_id",
                        column: x => x.replaced_by_token_id,
                        principalSchema: "auth",
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_replaced_by_token_id",
                schema: "auth",
                table: "UserSessions",
                column: "replaced_by_token_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_token_hash_user_id",
                schema: "auth",
                table: "UserSessions",
                columns: new[] { "token_hash", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_user_id",
                schema: "auth",
                table: "UserSessions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSessions",
                schema: "auth");
        }
    }
}
