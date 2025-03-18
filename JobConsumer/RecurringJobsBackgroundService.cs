using Common;
using MassTransit;
using MassTransit.Contracts.JobService;
using MassTransit.JobService;
using MassTransit.JobService.Messages;

namespace JobConsumer;

// This one-shot background service runs once per application start to ensure that the recurring jobs are configured
// correctly. Any existing recurring jobs whose settings are changed will be updated by AddOrUpdateRecurringJob.
internal sealed class RecurringJobsBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<RecurringJobsBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        var currentTenant = Environment.GetEnvironmentVariable("TENANT_NAME")!;

        await bus.SubmitRecurringJob(
            $"TestRecurringJob-{currentTenant}",
            new TestRecurringJob(InVar.Id, Guid.NewGuid(), InVar.Timestamp),
            x =>
            {
                x.CronExpression = "*/10 * * * * *"; // Every 10 seconds
            },
            properties: new Dictionary<string, object>
            {
                { TenantJobDistributionStrategy.TenantPropertyKey, currentTenant },
            },
            stoppingToken
        );

        logger.LogInformation("Recurring jobs configured");
    }
}

public static class MassTransitJobExtensions
{
    // This is a copy of RecurringJobConsumerExtensions.AddOrUpdateRecurringJob, modified to accept job properties
    public static async Task SubmitRecurringJob<T>(
        this IPublishEndpoint publishEndpoint,
        string jobName,
        T job,
        Action<IRecurringJobScheduleConfigurator>? configure = null,
        Dictionary<string, object>? properties = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var jobId = JobMetadataCache<T>.GenerateRecurringJobId(jobName);

        var schedule = new RecurringJobScheduleInfo();
        configure?.Invoke(schedule);

        schedule.Validate().ThrowIfContainsFailure("The schedule configuration is invalid:");

        await publishEndpoint.Publish<SubmitJob<T>>(
            new SubmitJobCommand<T>
            {
                JobId = jobId,
                Job = job,
                Schedule = schedule,
                Properties = properties,
            },
            cancellationToken
        );
    }
}
