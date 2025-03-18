using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSagaRunner.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "jobs");

            migrationBuilder.CreateTable(
                name: "job_saga",
                schema: "jobs",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<int>(type: "integer", nullable: false),
                    Submitted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServiceAddress = table.Column<string>(type: "text", nullable: true),
                    JobTimeout = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Job = table.Column<string>(type: "text", nullable: true),
                    JobTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    RetryAttempt = table.Column<int>(type: "integer", nullable: false),
                    Started = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Completed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Faulted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    JobSlotWaitToken = table.Column<Guid>(type: "uuid", nullable: true),
                    JobRetryDelayToken = table.Column<Guid>(type: "uuid", nullable: true),
                    IncompleteAttempts = table.Column<string>(type: "text", nullable: true),
                    LastProgressValue = table.Column<long>(type: "bigint", nullable: true),
                    LastProgressLimit = table.Column<long>(type: "bigint", nullable: true),
                    LastProgressSequenceNumber = table.Column<long>(type: "bigint", nullable: true),
                    JobState = table.Column<string>(type: "text", nullable: true),
                    JobProperties = table.Column<string>(type: "text", nullable: true),
                    CronExpression = table.Column<string>(type: "text", nullable: true),
                    TimeZoneId = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextStartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_saga", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "job_type_saga",
                schema: "jobs",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<int>(type: "integer", nullable: false),
                    ActiveJobCount = table.Column<int>(type: "integer", nullable: false),
                    ConcurrentJobLimit = table.Column<int>(type: "integer", nullable: false),
                    OverrideJobLimit = table.Column<int>(type: "integer", nullable: true),
                    OverrideLimitExpiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActiveJobs = table.Column<string>(type: "text", nullable: true),
                    Instances = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    GlobalConcurrentJobLimit = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_type_saga", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "job_attempt_saga",
                schema: "jobs",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<int>(type: "integer", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    RetryAttempt = table.Column<int>(type: "integer", nullable: false),
                    ServiceAddress = table.Column<string>(type: "text", nullable: true),
                    InstanceAddress = table.Column<string>(type: "text", nullable: true),
                    Started = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Faulted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusCheckTokenId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_attempt_saga", x => x.CorrelationId);
                    table.ForeignKey(
                        name: "FK_job_attempt_saga_job_saga_JobId",
                        column: x => x.JobId,
                        principalSchema: "jobs",
                        principalTable: "job_saga",
                        principalColumn: "CorrelationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_job_attempt_saga_JobId_RetryAttempt",
                schema: "jobs",
                table: "job_attempt_saga",
                columns: new[] { "JobId", "RetryAttempt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_attempt_saga",
                schema: "jobs");

            migrationBuilder.DropTable(
                name: "job_type_saga",
                schema: "jobs");

            migrationBuilder.DropTable(
                name: "job_saga",
                schema: "jobs");
        }
    }
}
