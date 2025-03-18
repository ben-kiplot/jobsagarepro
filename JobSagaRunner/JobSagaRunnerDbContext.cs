using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace JobSagaRunner;

public class JobSagaRunnerDbContext(DbContextOptions<JobSagaRunnerDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MassTransit Jobs
        const string jobsSchemaName = "jobs";
        new JobTypeSagaMap(false).Configure(modelBuilder);
        modelBuilder
            .Entity<JobTypeSaga>()
            .ToTable("job_type_saga")
            .Metadata.SetSchema(jobsSchemaName);

        new JobSagaMap(false).Configure(modelBuilder);
        modelBuilder.Entity<JobSaga>().ToTable("job_saga").Metadata.SetSchema(jobsSchemaName);

        new JobAttemptSagaMap(false).Configure(modelBuilder);
        modelBuilder
            .Entity<JobAttemptSaga>()
            .ToTable("job_attempt_saga")
            .Metadata.SetSchema(jobsSchemaName);
    }
}
