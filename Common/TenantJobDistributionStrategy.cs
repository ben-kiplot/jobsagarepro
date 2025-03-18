using MassTransit;
using MassTransit.Contracts.JobService;
using MassTransit.JobService;
using Microsoft.Extensions.Logging;

namespace Common;

/// <summary>
/// A MassTransit job distribution strategy that distributes jobs based on tenant - ensures that jobs for a specific
/// tenant are only executed using registered MassTransit job service instances for that tenant.
/// </summary>
public class TenantJobDistributionStrategy(ILogger<TenantJobDistributionStrategy> logger)
    : IJobDistributionStrategy
{
    public const string DistributionStrategyKey = "DistributionStrategy";
    public const string DistributionStrategyValue = nameof(TenantJobDistributionStrategy);
    public const string TenantPropertyKey = "Tenant";

    public Task<ActiveJob?> IsJobSlotAvailable(
        ConsumeContext<AllocateJobSlot> context,
        JobTypeInfo jobTypeInfo
    )
    {
        jobTypeInfo.Properties.TryGetValue(DistributionStrategyKey, out var strategy);

        return strategy switch
        {
            DistributionStrategyValue => Tenant(context, jobTypeInfo),
            _ => DefaultJobDistributionStrategy.Instance.IsJobSlotAvailable(context, jobTypeInfo),
        };
    }

    private Task<ActiveJob?> Tenant(
        ConsumeContext<AllocateJobSlot> context,
        JobTypeInfo jobTypeInfo
    )
    {
        string? jobTenant = null;
        if (
            context.Message.JobProperties?.TryGetValue(
                TenantPropertyKey,
                out var jobTenantPropertyObject
            ) ?? false
        )
        {
            jobTenant = jobTenantPropertyObject as string;
        }
        logger.LogDebug(
            "Executing Tenant job distribution strategy. Job Tenant: {JobTenant}, JobType total instances: {count}",
            jobTenant,
            jobTypeInfo.Instances.Count
        );

        var instances =
            from i in jobTypeInfo.Instances
            join a in jobTypeInfo.ActiveJobs on i.Key equals a.InstanceAddress into ai
            where
                (ai.Count() < jobTypeInfo.ConcurrentJobLimit && string.IsNullOrEmpty(jobTenant))
                || (
                    (i.Value.Properties?.TryGetValue(TenantPropertyKey, out var mt) ?? false)
                    && mt is string instanceTenant
                    && instanceTenant == jobTenant
                )
            orderby ai.Count(), i.Value.Used
            select new
            {
                Instance = i.Value,
                InstanceAddress = i.Key,
                InstanceCount = ai.Count(),
            };
        instances = instances.ToList();

        logger.LogDebug(
            "Found {InstanceCount} instances for Tenant {JobTenant}",
            instances.Count(),
            jobTenant
        );

        var firstInstance = instances.FirstOrDefault();
        if (firstInstance == null)
            return Task.FromResult<ActiveJob?>(null);

        return Task.FromResult<ActiveJob?>(
            new ActiveJob
            {
                JobId = context.Message.JobId,
                InstanceAddress = firstInstance.InstanceAddress
            }
        );
    }
}
