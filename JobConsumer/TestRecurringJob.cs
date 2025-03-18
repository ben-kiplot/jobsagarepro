using Common;
using MassTransit;

namespace JobConsumer;

public record TestRecurringJob(Guid Id, Guid CorrelationId, DateTime Timestamp);

public class TestRecurringJobConsumer(ILogger<TestRecurringJobConsumer> logger)
    : IJobConsumer<TestRecurringJob>
{
    public Task Run(JobContext<TestRecurringJob> context)
    {
        logger.LogInformation("Received the Recurring Job {JobName}", nameof(TestRecurringJob));
        return Task.CompletedTask;
    }
}

public class TestRecurringJobConsumerDefinition : ConsumerDefinition<TestRecurringJobConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TestRecurringJobConsumer> consumerConfigurator,
        IRegistrationContext context
    )
    {
        var currentTenant = Environment.GetEnvironmentVariable("TENANT_NAME")!;
        consumerConfigurator.Options<JobOptions<TestRecurringJob>>(options =>
            options
                .SetRetry(r => r.Interval(3, TimeSpan.FromSeconds(30)))
                .SetJobTimeout(TimeSpan.FromSeconds(10))
                .SetGlobalConcurrentJobLimit(1)
                .SetJobTypeProperties(p =>
                    p.Set(
                        TenantJobDistributionStrategy.DistributionStrategyKey,
                        TenantJobDistributionStrategy.DistributionStrategyValue
                    )
                )
                .SetInstanceProperties(p =>
                    p.Set(TenantJobDistributionStrategy.TenantPropertyKey, currentTenant)
                )
        );
    }
}
