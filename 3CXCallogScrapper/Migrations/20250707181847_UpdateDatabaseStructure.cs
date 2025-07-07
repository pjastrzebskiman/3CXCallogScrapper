using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _3CXCallogScrapper.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CallLogs",
                table: "CallLogs");

            migrationBuilder.AddColumn<string>(
                name: "MainCallHistoryId",
                table: "CallLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CallHistoryId",
                table: "CallLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallType",
                table: "CallLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CdrId",
                table: "CallLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direction",
                table: "CallLogs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CallLogs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CallLogs",
                table: "CallLogs",
                column: "MainCallHistoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CallLogs",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "MainCallHistoryId",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "CallHistoryId",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "CallType",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "CdrId",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CallLogs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CallLogs",
                table: "CallLogs",
                column: "SegmentId");
        }
    }
}
