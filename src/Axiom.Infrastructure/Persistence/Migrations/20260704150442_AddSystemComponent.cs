using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Axiom.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemComponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemComponents",
                columns: table => new
                {
                    SystemComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SystemId = table.Column<long>(type: "bigint", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleDescription = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemComponents", x => x.SystemComponentId);
                    table.ForeignKey(
                        name: "FK_SystemComponents_Systems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "Systems",
                        principalColumn: "SystemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemComponents_TechnicalComponents_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "TechnicalComponents",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemComponents_ComponentId",
                table: "SystemComponents",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemComponents_IsOwner",
                table: "SystemComponents",
                column: "IsOwner");

            migrationBuilder.CreateIndex(
                name: "IX_SystemComponents_SystemId",
                table: "SystemComponents",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemComponents_SystemId_ComponentId",
                table: "SystemComponents",
                columns: new[] { "SystemId", "ComponentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemComponents");
        }
    }
}
