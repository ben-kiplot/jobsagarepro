using System.ComponentModel.DataAnnotations;

namespace Common;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required]
    public RabbitMqAddress Address { get; set; } = new();

    [Required]
    public RabbitMqCredentials Credentials { get; set; } = new();

    [Required]
    public RabbitMqEndpoints Endpoints { get; set; } = new();

    public int RequestClientTimeoutSeconds { get; set; } = 60;
}

public sealed class RabbitMqAddress
{
    [Required]
    public string Host { get; set; } = string.Empty;

    [Required]
    public string VirtualHost { get; set; } = string.Empty;
}

public sealed class RabbitMqCredentials
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class RabbitMqEndpoints
{
    [Required]
    public string EndpointNamePrefix { get; set; } = null!;
}

