using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatform.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnalysisJson",
                table: "Resumes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisStatus",
                table: "Resumes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "NotAnalyzed");

            migrationBuilder.AddColumn<string>(
                name: "AnalysisStrategy",
                table: "Resumes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AnalyzedAtUtc",
                table: "Resumes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisJson",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AnalysisStatus",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AnalysisStrategy",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "AnalyzedAtUtc",
                table: "Resumes");
        }
    }
}
