namespace EventSourcedPM.Configuration;

using System;
using System.Text;

public class EventStoreSettings
{
    public EndpointSettings Endpoint { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ConnectionOptions { get; set; }

    private string GetConnectionOptions()
        => string.IsNullOrEmpty(ConnectionOptions) ? string.Empty : $"?{ConnectionOptions}";
    public string GetConnectionString()
        => $"{Endpoint?.Scheme}://{Username}:{Password}@{Endpoint?.Host}:{Endpoint?.Port}{GetConnectionOptions()}";
    public bool HealthCheck { get; set; }

    public override string ToString() =>
        new StringBuilder()
            .AppendSettingValue(() => HealthCheck)
            .AppendSettingValue(() => Endpoint.Host)
            .AppendSettingValue(() => Endpoint.Port)
            .AppendSettingValue(() => Username)
            .AppendSettingValue(() => ConnectionOptions)
            .ToString();
}

public class PostgresSettings
{
    public EndpointSettings Endpoint { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Database { get; set; }
    public string ConnectionOptions { get; set; }
    public bool AutoMigrate { get; set; } = true;

    public string GetConnectionString() =>
        $"Server={Endpoint.Host};Port={Endpoint.Port};Database={Database};User Id={Username};Password={Password};{ConnectionOptions}";

    public bool HealthCheck { get; set; }

    public override string ToString() =>
        new StringBuilder()
            .AppendSettingValue(() => HealthCheck)
            .AppendSettingValue(() => Endpoint.Host)
            .AppendSettingValue(() => Endpoint.Port)
            .AppendSettingValue(() => Username)
            .AppendSettingValue(() => Database)
            .AppendSettingValue(() => ConnectionOptions)
            .AppendSettingValue(() => AutoMigrate)
            .ToString();
}

public class RabbitMqSettings
{
    public EndpointSettings Endpoint { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string VHost { get; set; }

    public string GetRabbitMqUrl() => $"{Endpoint}/{VHost}";

    public string GetAmqpUrl() =>
        $"amqp://{Username.ToUrlPart()}:{Password.ToUrlPart()}@{Endpoint.Host}:{Endpoint.Port}/{VHost}";

    public bool HealthCheck { get; set; }

    public override string ToString() =>
        new StringBuilder()
            .AppendSettingValue(() => HealthCheck)
            .AppendSettingValue(() => Endpoint)
            .AppendSettingValue(() => VHost)
            .AppendSettingValue(() => Username)
            .ToString();
}

public class MassTransitSettings
{
    public int PrefetchCount { get; set; } = 1;
    public int ConcurrencyLimit { get; set; } = 0;
    public RetrySettings Retry { get; set; }

    public override string ToString() =>
        new StringBuilder()
            .AppendSettingValue(() => PrefetchCount)
            .AppendSettingValue(() => ConcurrencyLimit)
            .AppendSettingValue(() => Retry)
            .ToString();
}

public class EndpointSettings
{
    public string Scheme { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }

    public override string ToString() => $"{Scheme}://{Host}:{Port}";
}

public class RetrySettings
{
    public int Limit { get; set; } = 5;
    public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan IntervalMin { get; set; } = TimeSpan.FromMilliseconds(10);
    public TimeSpan IntervalMax { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan IntervalDelta { get; set; } = TimeSpan.FromMilliseconds(200);
    public bool FastFirst => IntervalMin == TimeSpan.Zero;

    public override string ToString() =>
        $"Limit: {Limit} / Timeout: {Timeout} [Min: {IntervalMin}; Max: {IntervalMax}; Delta: {IntervalDelta}]";
}
