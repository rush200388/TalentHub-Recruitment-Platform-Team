using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatform.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InterviewFeedback_InterviewId",
                table: "InterviewFeedback");

            migrationBuilder.DropIndex(
                name: "IX_CandidateEvaluations_JobApplicationId",
                table: "CandidateEvaluations");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Interviews",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MeetingLink",
                table: "Interviews",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Interviews",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalCalendarEventId",
                table: "Interviews",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CalendarProvider",
                table: "Interviews",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterviewerUserId",
                table: "Interviews",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Recommendation",
                table: "InterviewFeedback",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "InterviewFeedback",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "CandidateEvaluations",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InterviewerUserId_StartTimeUtc_EndTimeUtc",
                table: "Interviews",
                columns: new[] { "InterviewerUserId", "StartTimeUtc", "EndTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewFeedback_InterviewId_ReviewerUserId",
                table: "InterviewFeedback",
                columns: new[] { "InterviewId", "ReviewerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_JobApplicationId_EvaluatorUserId",
                table: "CandidateEvaluations",
                columns: new[] { "JobApplicationId", "EvaluatorUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Interviews_InterviewerUserId_StartTimeUtc_EndTimeUtc",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_InterviewFeedback_InterviewId_ReviewerUserId",
                table: "InterviewFeedback");

            migrationBuilder.DropIndex(
                name: "IX_CandidateEvaluations_JobApplicationId_EvaluatorUserId",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "InterviewerUserId",
                table: "Interviews");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Interviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3000)",
                oldMaxLength: 3000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MeetingLink",
                table: "Interviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Interviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalCalendarEventId",
                table: "Interviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CalendarProvider",
                table: "Interviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Recommendation",
                table: "InterviewFeedback",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "InterviewFeedback",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3000)",
                oldMaxLength: 3000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "CandidateEvaluations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3000)",
                oldMaxLength: 3000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewFeedback_InterviewId",
                table: "InterviewFeedback",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_JobApplicationId",
                table: "CandidateEvaluations",
                column: "JobApplicationId");
        }
    }
}
