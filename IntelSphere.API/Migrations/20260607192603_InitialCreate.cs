using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BreachSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceName = table.Column<string>(type: "TEXT", nullable: false),
                    BreachDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreachSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonitoredDomains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DomainName = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", nullable: false),
                    RiskScore = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredDomains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeakedCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    BreachSourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscoveredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeakedCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeakedCredentials_BreachSources_BreachSourceId",
                        column: x => x.BreachSourceId,
                        principalTable: "BreachSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeakedCredentials_BreachSourceId",
                table: "LeakedCredentials",
                column: "BreachSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_LeakedCredentials_Domain",
                table: "LeakedCredentials",
                column: "Domain");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeakedCredentials");

            migrationBuilder.DropTable(
                name: "MonitoredDomains");

            migrationBuilder.DropTable(
                name: "BreachSources");
        }
    }
}
