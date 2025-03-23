using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace careerhive.infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSavedTableToDb : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameIndex(
            name: "IX_UserSubscriptions_UserId",
            table: "UserSubscriptions",
            newName: "IX_UserSubscription_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_UserSubscriptions_Email",
            table: "UserSubscriptions",
            newName: "IX_UserSubscription_Email");

        migrationBuilder.CreateTable(
            name: "SavedJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SavedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SavedJobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_SavedJobs_AspNetUsers_SavedByUserId",
                    column: x => x.SavedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SavedJobs_Jobs_JobId",
                    column: x => x.JobId,
                    principalTable: "Jobs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_User_Email",
            table: "AspNetUsers",
            column: "Email",
            unique: true,
            filter: "[Email] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_User_UserName",
            table: "AspNetUsers",
            column: "UserName",
            unique: true,
            filter: "[UserName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_SavedJobs_JobId",
            table: "SavedJobs",
            column: "JobId");

        migrationBuilder.CreateIndex(
            name: "IX_SavedJobs_SavedByUserId",
            table: "SavedJobs",
            column: "SavedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_SavedJobs_User_Job",
            table: "SavedJobs",
            columns: new[] { "SavedByUserId", "JobId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SavedJobs");

        migrationBuilder.DropIndex(
            name: "IX_User_Email",
            table: "AspNetUsers");

        migrationBuilder.DropIndex(
            name: "IX_User_UserName",
            table: "AspNetUsers");

        migrationBuilder.RenameIndex(
            name: "IX_UserSubscription_UserId",
            table: "UserSubscriptions",
            newName: "IX_UserSubscriptions_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_UserSubscription_Email",
            table: "UserSubscriptions",
            newName: "IX_UserSubscriptions_Email");
    }
}
