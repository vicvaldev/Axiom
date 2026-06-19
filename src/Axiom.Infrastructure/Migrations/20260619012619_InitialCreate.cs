using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Axiom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueStates",
                columns: table => new
                {
                    StateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueStates", x => x.StateId);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeStates",
                columns: table => new
                {
                    StateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeStates", x => x.StateId);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeTags",
                columns: table => new
                {
                    KnowledgeTagId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeTags", x => x.KnowledgeTagId);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeTypes",
                columns: table => new
                {
                    TypeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeTypes", x => x.TypeId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Systems",
                columns: table => new
                {
                    SystemId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EAI = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Systems", x => x.SystemId);
                    table.ForeignKey(
                        name: "FK_Systems_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Summary = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    RitmNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    IncidentNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    SystemId = table.Column<long>(type: "bigint", nullable: false),
                    Problem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Analysis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.IssueId);
                    table.ForeignKey(
                        name: "FK_Issues_IssueStates_StateId",
                        column: x => x.StateId,
                        principalTable: "IssueStates",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Systems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "Systems",
                        principalColumn: "SystemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Knowledges",
                columns: table => new
                {
                    KnowledgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Summary = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KnowledgeTypeId = table.Column<long>(type: "bigint", nullable: false),
                    KnowledgeStateId = table.Column<int>(type: "int", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VersionNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Knowledges", x => x.KnowledgeId);
                    table.ForeignKey(
                        name: "FK_Knowledges_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Knowledges_KnowledgeStates_KnowledgeStateId",
                        column: x => x.KnowledgeStateId,
                        principalTable: "KnowledgeStates",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Knowledges_KnowledgeTypes_KnowledgeTypeId",
                        column: x => x.KnowledgeTypeId,
                        principalTable: "KnowledgeTypes",
                        principalColumn: "TypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Knowledges_Systems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "Systems",
                        principalColumn: "SystemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Knowledges_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeKnowledgeTags",
                columns: table => new
                {
                    KnowledgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KnowledgeTagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeKnowledgeTags", x => new { x.KnowledgeId, x.KnowledgeTagId });
                    table.ForeignKey(
                        name: "FK_KnowledgeKnowledgeTags_KnowledgeTags_KnowledgeTagId",
                        column: x => x.KnowledgeTagId,
                        principalTable: "KnowledgeTags",
                        principalColumn: "KnowledgeTagId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KnowledgeKnowledgeTags_Knowledges_KnowledgeId",
                        column: x => x.KnowledgeId,
                        principalTable: "Knowledges",
                        principalColumn: "KnowledgeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_CreatedByUserId",
                table: "Issues",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_IncidentNumber",
                table: "Issues",
                column: "IncidentNumber",
                unique: true,
                filter: "[IncidentNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_RitmNumber",
                table: "Issues",
                column: "RitmNumber",
                unique: true,
                filter: "[RitmNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_StateId",
                table: "Issues",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_SystemId",
                table: "Issues",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueStates_Code",
                table: "IssueStates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeKnowledgeTags_KnowledgeTagId",
                table: "KnowledgeKnowledgeTags",
                column: "KnowledgeTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Knowledges_CreatedByUserId",
                table: "Knowledges",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Knowledges_IssueId",
                table: "Knowledges",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Knowledges_KnowledgeStateId",
                table: "Knowledges",
                column: "KnowledgeStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Knowledges_KnowledgeTypeId",
                table: "Knowledges",
                column: "KnowledgeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Knowledges_SystemId",
                table: "Knowledges",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeStates_Code",
                table: "KnowledgeStates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeTags_TagName",
                table: "KnowledgeTags",
                column: "TagName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeTypes_Code",
                table: "KnowledgeTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Systems_OwnerUserId",
                table: "Systems",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnowledgeKnowledgeTags");

            migrationBuilder.DropTable(
                name: "KnowledgeTags");

            migrationBuilder.DropTable(
                name: "Knowledges");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "KnowledgeStates");

            migrationBuilder.DropTable(
                name: "KnowledgeTypes");

            migrationBuilder.DropTable(
                name: "IssueStates");

            migrationBuilder.DropTable(
                name: "Systems");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
