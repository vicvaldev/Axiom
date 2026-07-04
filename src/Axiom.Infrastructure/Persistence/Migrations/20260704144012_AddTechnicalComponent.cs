using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Axiom.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicalComponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TechnicalComponents",
                columns: table => new
                {
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    TechnicalName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    ComponentType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Environment = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Criticality = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SystemId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalComponents", x => x.ComponentId);
                    table.ForeignKey(
                        name: "FK_TechnicalComponents_Systems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "Systems",
                        principalColumn: "SystemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalComponents_ComponentType",
                table: "TechnicalComponents",
                column: "ComponentType");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalComponents_Environment",
                table: "TechnicalComponents",
                column: "Environment");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalComponents_SystemId",
                table: "TechnicalComponents",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalComponents_TechnicalName",
                table: "TechnicalComponents",
                column: "TechnicalName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TechnicalComponents");
        }
    }
}
