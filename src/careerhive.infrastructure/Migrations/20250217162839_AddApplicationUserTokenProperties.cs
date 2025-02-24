using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace careerhive.infrastructure.Migrations;

/// <inheritdoc />
public partial class AddApplicationUserTokenProperties : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationUserTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TokenType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TokenValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                UsedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApplicationUserTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApplicationUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationUserTokens_UserId",
            table: "ApplicationUserTokens",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ApplicationUserTokens");
    }
}
