namespace EventSourcedPM.Configuration;

using System;
using System.Text;

public class EventStoreSettings
{
    public EndpointSettings Endpoint { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
    public string ConnectionOptions { get; init; }

    private string GetConnectionOptions()
        => string.IsNullOrEmpty(ConnectionOptions) ? string.Empty : $"?{ConnectionOptions}";
    public string GetConnectionString()
        => $"{Endpoint?.Scheme}://{Username}:{Password}@{Endpoint?.Host}:{Endpoint?.Port}{GetConnectionOptions()}";
    public bool HealthCheck { get; init; }

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
    public EndpointSettings Endpoint { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
    public string Database { get; init; }
    public string ConnectionOptions { get; init; }
    public bool AutoMigrate { get; init; } = true;

    public string GetConnectionString() =>
        $"Server={Endpoint.Host};Port={Endpoint.Port};Database={Database};User Id={Username};Password={Password};{ConnectionOptions}";

    public bool HealthCheck { get; init;}

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
    public EndpointSettings Endpoint { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
    public string VHost { get; init; }

    public string GetRabbitMqUrl() => $"{Endpoint}/{VHost}";

    public string GetAmqpUrl() =>
        $"amqp://{Username.ToUrlPart()}:{Password.ToUrlPart()}@{Endpoint.Host}:{Endpoint.Port}/{VHost}";

    public bool HealthCheck { get; init; }

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
    public int PrefetchCount { get; init; } = 1;
    public int ConcurrencyLimit { get; init; } = 0;
    public RetrySettings Retry { get; init; }

    public override string ToString() =>
        new StringBuilder()
            .AppendSettingValue(() => PrefetchCount)
            .AppendSettingValue(() => ConcurrencyLimit)
            .AppendSettingValue(() => Retry)
            .ToString();
}

public class EndpointSettings
{
    public string Scheme { get; init; }
    public string Host { get; init;}
    public int Port { get; init;}

    public override string ToString() => $"{Scheme}://{Host}:{Port}";
}

public class RetrySettings
{
    public int Limit { get; init;} = 5;
    public TimeSpan? Timeout { get; init;} = TimeSpan.FromSeconds(30);
    public TimeSpan IntervalMin { get; init;} = TimeSpan.FromMilliseconds(10);
    public TimeSpan IntervalMax { get; init;} = TimeSpan.FromSeconds(5);
    public TimeSpan IntervalDelta { get; init;} = TimeSpan.FromMilliseconds(200);
    public bool FastFirst => IntervalMin == TimeSpan.Zero;

    public override string ToString() =>
        $"Limit: {Limit} / Timeout: {Timeout} [Min: {IntervalMin}; Max: {IntervalMax}; Delta: {IntervalDelta}]";
}
