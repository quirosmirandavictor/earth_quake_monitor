using EarthquakeMonitor.Analytics.Application.Abstractions;
using EarthquakeMonitor.Analytics.Domain;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace EarthquakeMonitor.Analytics.Infrastructure.Persistence.Oracle;

public sealed class OracleAnalyticsRepository(IConfiguration configuration) : IAnalyticsRepository
{
    private const string SummarySql = """
        SELECT COUNT(*) AS EVENT_COUNT,
               MAX(MAGNITUDE) AS MAXIMUM_MAGNITUDE,
               AVG(MAGNITUDE) AS AVERAGE_MAGNITUDE
        FROM EARTHQUAKES
        WHERE ORIGIN_TIME >= :from_time
          AND ORIGIN_TIME < :to_time
        """;

    public async Task<EarthquakeSummary> GetSummaryAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        if (to <= from)
            throw new ArgumentException("The end of the period must be later than its start.", nameof(to));

        var connectionString = configuration.GetConnectionString("Oracle")
            ?? throw new InvalidOperationException("ConnectionStrings:Oracle is not configured.");

        var tnsAdmin = new OracleConnectionStringBuilder(connectionString).TnsAdmin;
        if (!string.IsNullOrWhiteSpace(tnsAdmin))
            OracleConfiguration.TnsAdmin = tnsAdmin;

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new OracleCommand(SummarySql, connection)
        {
            BindByName = true
        };
        command.Parameters.Add("from_time", from.UtcDateTime);
        command.Parameters.Add("to_time", to.UtcDateTime);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new EarthquakeSummary(from, to, 0, null, null);

        return new EarthquakeSummary(
            from,
            to,
            Convert.ToInt32(reader.GetValue(0)),
            reader.IsDBNull(1) ? null : reader.GetDecimal(1),
            reader.IsDBNull(2) ? null : reader.GetDecimal(2));
    }
}
