using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _3CXCallogScrapper.Migrations
{
    /// <inheritdoc />
    public partial class AddStartTimeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_StartTime",
                table: "CallLogs",
                column: "StartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CallLogs_StartTime",
                table: "CallLogs");
        }
    }
}
