using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.ImageToolsModule.Data.PostgreSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThumbnailOption",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FileSuffix = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResizeMethod = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    BackgroundColor = table.Column<string>(type: "text", nullable: true),
                    AnchorPosition = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    JpegQuality = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThumbnailOption", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThumbnailTask",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    WorkPath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    LastRun = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThumbnailTask", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThumbnailTaskOption",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ThumbnailTaskId = table.Column<string>(type: "character varying(128)", nullable: false),
                    ThumbnailOptionId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThumbnailTaskOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThumbnailTaskOption_ThumbnailOption_ThumbnailOptionId",
                        column: x => x.ThumbnailOptionId,
                        principalTable: "ThumbnailOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThumbnailTaskOption_ThumbnailTask_ThumbnailTaskId",
                        column: x => x.ThumbnailTaskId,
                        principalTable: "ThumbnailTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThumbnailTaskOption_ThumbnailOptionId",
                table: "ThumbnailTaskOption",
                column: "ThumbnailOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ThumbnailTaskOption_ThumbnailTaskId",
                table: "ThumbnailTaskOption",
                column: "ThumbnailTaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThumbnailTaskOption");

            migrationBuilder.DropTable(
                name: "ThumbnailOption");

            migrationBuilder.DropTable(
                name: "ThumbnailTask");
        }
    }
}
