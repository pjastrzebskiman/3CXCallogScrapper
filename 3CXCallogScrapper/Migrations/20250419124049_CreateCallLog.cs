using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _3CXCallogScrapper.Migrations
{
    /// <inheritdoc />
    public partial class CreateCallLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CallLogs",
                columns: table => new
                {
                    SegmentId = table.Column<int>(type: "integer", nullable: false),
                    CallId = table.Column<int>(type: "integer", nullable: false),
                    Indent = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceDn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceCallerId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DestinationType = table.Column<int>(type: "integer", nullable: false),
                    DestinationDn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DestinationCallerId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DestinationDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    ActionDnType = table.Column<int>(type: "integer", nullable: true),
                    ActionDnDn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActionDnCallerId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActionDnDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RingingDuration = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TalkingDuration = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CallCost = table.Column<double>(type: "double precision", nullable: true),
                    Answered = table.Column<bool>(type: "boolean", nullable: false),
                    RecordingUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SubrowDescNumber = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SrcRecId = table.Column<int>(type: "integer", nullable: true),
                    QualityReport = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallLogs", x => x.SegmentId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallLogs");
        }
    }
}
