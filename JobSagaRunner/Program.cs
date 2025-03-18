using Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JobSagaRunner;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<JobSagaRunnerDbContext>(
            (s, opt) =>
                opt.EnableDetailedErrors()
                    .UseNpgsql(builder.Configuration.GetConnectionString("JobSagaRunner"))
        );

        builder.Services.AddLogging(opt =>
        {
            opt.AddConsole(c =>
            {
                c.TimestampFormat = "[HH:mm:ss.ffffff] ";
            });
        });

        builder
            .Services.AddOptions<RabbitMqOptions>()
            .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.TryAddJobDistributionStrategy<TenantJobDistributionStrategy>();
        builder.Services.AddMassTransit(configure: configurator =>
        {
            configurator.AddDelayedMessageScheduler();

            configurator.AddConfigureEndpointsCallback(
                (_, _, receiveEndpointConfigurator) =>
                {
                    if (receiveEndpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
                        rmq.SetQuorumQueue();
                }
            );

            configurator
                .AddJobSagaStateMachines()
                .EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<JobSagaRunnerDbContext>();
                    r.UsePostgres();
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                });
            configurator.SetRabbitMqReplyToRequestClientFactory();

            var endpointNameFormatter = new KebabCaseEndpointNameFormatter("job-saga-runner");
            configurator.SetEndpointNameFormatter(endpointNameFormatter);
            configurator.UsingRabbitMq(
                (context, cfg) =>
                {
                    var rabbitMqOptions = context
                        .GetRequiredService<IOptions<RabbitMqOptions>>()
                        .Value;
                    cfg.Host(
                        new Uri(
                            $"rabbitmq://{rabbitMqOptions.Address.Host}/{rabbitMqOptions.Address.VirtualHost}".TrimEnd(
                                '/'
                            )
                        ),
                        "JobsRepro",
                        h =>
                        {
                            h.Username(rabbitMqOptions.Credentials.Username);
                            h.Password(rabbitMqOptions.Credentials.Password);
                        }
                    );
                    cfg.UseMessageRetry(r =>
                    {
                        r.Intervals(100, 500, 1000, 2000);
                    });
                    cfg.UseDelayedMessageScheduler();
                    cfg.UseJobSagaPartitionKeyFormatters();
                    cfg.ConfigureEndpoints(context);
                }
            );
        });

        var app = builder.Build();
        await app.RunAsync();
    }
}
