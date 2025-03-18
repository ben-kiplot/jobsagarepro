using Common;
using MassTransit;

namespace JobConsumer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
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

        builder.Services.AddMassTransit(configurator =>
        {
            var rabbitMqOptions = builder
                .Configuration.GetSection(RabbitMqOptions.SectionName)
                .Get<RabbitMqOptions>()!;
            var endpointNameFormatter = new KebabCaseEndpointNameFormatter(
                Environment.GetEnvironmentVariable("TENANT_NAME") ?? "DEFAULT_TENANT"
            );
            configurator.SetEndpointNameFormatter(endpointNameFormatter);

            configurator.AddConfigureEndpointsCallback(
                (_, _, receiveEndpointConfigurator) =>
                {
                    if (receiveEndpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
                        rmq.SetQuorumQueue();
                }
            );

            configurator.UsingRabbitMq(
                (context, cfg) =>
                {
                    cfg.AutoStart = true;
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

                    cfg.UseMessageScope(context);
                    cfg.ConfigureEndpoints(context);
                }
            );

            configurator.AddConsumer<
                TestRecurringJobConsumer,
                TestRecurringJobConsumerDefinition
            >();
        });

        builder.Services.AddHostedService<RecurringJobsBackgroundService>();
        var app = builder.Build();
        await app.RunAsync();
    }
}
