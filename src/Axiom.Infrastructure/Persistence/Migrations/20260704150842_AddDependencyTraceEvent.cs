using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Axiom.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDependencyTraceEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DependencyTraceEvents",
                columns: table => new
                {
                    TraceEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedKnowledgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedRitmNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    RelatedChangeNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependencyTraceEvents", x => x.TraceEventId);
                    table.ForeignKey(
                        name: "FK_DependencyTraceEvents_ComponentDependencies_DependencyId",
                        column: x => x.DependencyId,
                        principalTable: "ComponentDependencies",
                        principalColumn: "DependencyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DependencyTraceEvents_CreatedAt",
                table: "DependencyTraceEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DependencyTraceEvents_DependencyId",
                table: "DependencyTraceEvents",
                column: "DependencyId");

            migrationBuilder.CreateIndex(
                name: "IX_DependencyTraceEvents_EventType",
                table: "DependencyTraceEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_DependencyTraceEvents_RelatedIssueId",
                table: "DependencyTraceEvents",
                column: "RelatedIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_DependencyTraceEvents_RelatedKnowledgeId",
                table: "DependencyTraceEvents",
                column: "RelatedKnowledgeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DependencyTraceEvents");
        }
    }
}
