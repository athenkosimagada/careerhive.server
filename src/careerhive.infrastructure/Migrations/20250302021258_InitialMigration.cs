using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace careerhive.infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Title",
            table: "Jobs",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateTable(
            name: "UserSubscriptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserSubscriptions_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Jobs_CreatedAt",
            table: "Jobs",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Jobs_Title",
            table: "Jobs",
            column: "Title");

        migrationBuilder.CreateIndex(
            name: "IX_UserSubscriptions_Email",
            table: "UserSubscriptions",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserSubscriptions_UserId",
            table: "UserSubscriptions",
            column: "UserId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserSubscriptions");

        migrationBuilder.DropIndex(
            name: "IX_Jobs_CreatedAt",
            table: "Jobs");

        migrationBuilder.DropIndex(
            name: "IX_Jobs_Title",
            table: "Jobs");

        migrationBuilder.AlterColumn<string>(
            name: "Title",
            table: "Jobs",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");
    }
}
