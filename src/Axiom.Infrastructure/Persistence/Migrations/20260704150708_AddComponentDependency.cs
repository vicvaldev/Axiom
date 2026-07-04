using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Axiom.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentDependency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComponentDependencies",
                columns: table => new
                {
                    DependencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependencyType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Criticality = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentDependencies", x => x.DependencyId);
                    table.ForeignKey(
                        name: "FK_ComponentDependencies_TechnicalComponents_SourceComponentId",
                        column: x => x.SourceComponentId,
                        principalTable: "TechnicalComponents",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentDependencies_TechnicalComponents_TargetComponentId",
                        column: x => x.TargetComponentId,
                        principalTable: "TechnicalComponents",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDependencies_DependencyType",
                table: "ComponentDependencies",
                column: "DependencyType");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDependencies_Source_Target_Type",
                table: "ComponentDependencies",
                columns: new[] { "SourceComponentId", "TargetComponentId", "DependencyType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDependencies_SourceComponentId",
                table: "ComponentDependencies",
                column: "SourceComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDependencies_Status",
                table: "ComponentDependencies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentDependencies_TargetComponentId",
                table: "ComponentDependencies",
                column: "TargetComponentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentDependencies");
        }
    }
}
