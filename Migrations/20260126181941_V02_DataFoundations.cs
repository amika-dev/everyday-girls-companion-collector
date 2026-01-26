using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EverydayGirlsCompanionCollector.Migrations
{
    /// <inheritdoc />
    public partial class V02_DataFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Charm",
                table: "UserGirls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Focus",
                table: "UserGirls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Vitality",
                table: "UserGirls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyBalance",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Townie");

            migrationBuilder.AddColumn<string>(
                name: "DisplayNameNormalized",
                table: "AspNetUsers",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "TOWNIE");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDisplayNameChangeUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FriendRelationships",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FriendUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateAddedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRelationships", x => new { x.UserId, x.FriendUserId });
                    table.ForeignKey(
                        name: "FK_FriendRelationships_AspNetUsers_FriendUserId",
                        column: x => x.FriendUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FriendRelationships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TownLocations",
                columns: table => new
                {
                    TownLocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrimarySkill = table.Column<byte>(type: "tinyint", nullable: false),
                    BaseDailyBondGain = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    BaseDailyCurrencyGain = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    BaseDailySkillGain = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    IsLockedByDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UnlockCost = table.Column<int>(type: "int", nullable: false, defaultValue: 50)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TownLocations", x => x.TownLocationId);
                });

            migrationBuilder.CreateTable(
                name: "CompanionAssignments",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GirlId = table.Column<int>(type: "int", nullable: false),
                    TownLocationId = table.Column<int>(type: "int", nullable: false),
                    AssignedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssignments", x => new { x.UserId, x.GirlId });
                    table.ForeignKey(
                        name: "FK_CompanionAssignments_TownLocations_TownLocationId",
                        column: x => x.TownLocationId,
                        principalTable: "TownLocations",
                        principalColumn: "TownLocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssignments_UserGirls_UserId_GirlId",
                        columns: x => new { x.UserId, x.GirlId },
                        principalTable: "UserGirls",
                        principalColumns: new[] { "UserId", "GirlId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTownLocationUnlocks",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TownLocationId = table.Column<int>(type: "int", nullable: false),
                    UnlockedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTownLocationUnlocks", x => new { x.UserId, x.TownLocationId });
                    table.ForeignKey(
                        name: "FK_UserTownLocationUnlocks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTownLocationUnlocks_TownLocations_TownLocationId",
                        column: x => x.TownLocationId,
                        principalTable: "TownLocations",
                        principalColumn: "TownLocationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DisplayNameNormalized",
                table: "AspNetUsers",
                column: "DisplayNameNormalized");

            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.AddCheckConstraint(
                    name: "CK_AspNetUsers_DisplayName_Valid",
                    table: "AspNetUsers",
                    sql: "LEN([DisplayName]) >= 4 AND LEN([DisplayName]) <= 16 AND [DisplayName] NOT LIKE '%[^a-zA-Z0-9]%'");
            }

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssignments_TownLocationId",
                table: "CompanionAssignments",
                column: "TownLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRelationships_FriendUserId",
                table: "FriendRelationships",
                column: "FriendUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTownLocationUnlocks_TownLocationId",
                table: "UserTownLocationUnlocks",
                column: "TownLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanionAssignments");

            migrationBuilder.DropTable(
                name: "FriendRelationships");

            migrationBuilder.DropTable(
                name: "UserTownLocationUnlocks");

            migrationBuilder.DropTable(
                name: "TownLocations");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DisplayNameNormalized",
                table: "AspNetUsers");

            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.DropCheckConstraint(
                    name: "CK_AspNetUsers_DisplayName_Valid",
                    table: "AspNetUsers");
            }

            migrationBuilder.DropColumn(
                name: "Charm",
                table: "UserGirls");

            migrationBuilder.DropColumn(
                name: "Focus",
                table: "UserGirls");

            migrationBuilder.DropColumn(
                name: "Vitality",
                table: "UserGirls");

            migrationBuilder.DropColumn(
                name: "CurrencyBalance",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayNameNormalized",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastDisplayNameChangeUtc",
                table: "AspNetUsers");
        }
    }
}
