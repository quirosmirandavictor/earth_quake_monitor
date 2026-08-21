using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace EarthquakeMonitor.Earthquakes.Infrastructure.Persistence.Oracle;

public sealed class OracleConnectionFactory
{
    private static readonly object ConfigurationLock = new();
    private static string? _configuredTnsAdmin;
    private readonly string _connectionString;

    public OracleConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Oracle")
            ?? throw new InvalidOperationException("ConnectionStrings:Oracle is not configured.");

        var tnsAdmin = new OracleConnectionStringBuilder(_connectionString).TnsAdmin;
        if (string.IsNullOrWhiteSpace(tnsAdmin))
            return;

        lock (ConfigurationLock)
        {
            if (_configuredTnsAdmin is null)
            {
                OracleConfiguration.TnsAdmin = tnsAdmin;
                _configuredTnsAdmin = tnsAdmin;
            }
            else if (!string.Equals(_configuredTnsAdmin, tnsAdmin, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Oracle Tns_Admin cannot be changed after Oracle has been configured.");
            }
        }
    }

    public async Task<OracleConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
