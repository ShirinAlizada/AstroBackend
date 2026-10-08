using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstroBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopForumI18nColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiryTime",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitLabelEn",
                table: "ShopProducts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitLabelRu",
                table: "ShopProducts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyEn",
                table: "ForumTopics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyRu",
                table: "ForumTopics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "ForumTopics",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleRu",
                table: "ForumTopics",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyEn",
                table: "ForumReplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyRu",
                table: "ForumReplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AiUsageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsageDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiUsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiUsageLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageLogs_UserId_UsageDate",
                table: "AiUsageLogs",
                columns: new[] { "UserId", "UsageDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiUsageLogs");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiryTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UnitLabelEn",
                table: "ShopProducts");

            migrationBuilder.DropColumn(
                name: "UnitLabelRu",
                table: "ShopProducts");

            migrationBuilder.DropColumn(
                name: "BodyEn",
                table: "ForumTopics");

            migrationBuilder.DropColumn(
                name: "BodyRu",
                table: "ForumTopics");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "ForumTopics");

            migrationBuilder.DropColumn(
                name: "TitleRu",
                table: "ForumTopics");

            migrationBuilder.DropColumn(
                name: "BodyEn",
                table: "ForumReplies");

            migrationBuilder.DropColumn(
                name: "BodyRu",
                table: "ForumReplies");
        }
    }
}
