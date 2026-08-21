using EarthquakeMonitor.Earthquakes.Application.Abstractions.Persistence;
using EarthquakeMonitor.Earthquakes.Domain;
using Oracle.ManagedDataAccess.Client;

namespace EarthquakeMonitor.Earthquakes.Infrastructure.Persistence.Oracle;

public sealed class OracleEarthquakeRepository(OracleConnectionFactory connectionFactory) : IEarthquakeRepository
{
    private const string UpsertSql = """
        MERGE INTO EARTHQUAKES target
        USING (SELECT :source SOURCE, :external_id EXTERNAL_ID FROM dual) incoming
        ON (target.SOURCE = incoming.SOURCE AND target.EXTERNAL_ID = incoming.EXTERNAL_ID)
        WHEN MATCHED THEN UPDATE SET ORIGIN_TIME=:origin_time, LATITUDE=:latitude, LONGITUDE=:longitude,
          DEPTH_KM=:depth_km, MAGNITUDE=:magnitude, MAGNITUDE_TYPE=:magnitude_type, PLACE=:place,
          EVENT_URL=:event_url, PROVIDER_UPDATED_AT=:provider_updated_at, PROVIDER_STATUS=:provider_status,
          TSUNAMI=:tsunami, ALERT_LEVEL=:alert_level, SIGNIFICANCE=:significance,
          RAW_PAYLOAD=:raw_payload, UPDATED_AT=:updated_at
        WHEN NOT MATCHED THEN INSERT (ID, SOURCE, EXTERNAL_ID, ORIGIN_TIME, LATITUDE, LONGITUDE,
          DEPTH_KM, MAGNITUDE, MAGNITUDE_TYPE, PLACE, EVENT_URL, PROVIDER_UPDATED_AT,
          PROVIDER_STATUS, TSUNAMI, ALERT_LEVEL, SIGNIFICANCE, RAW_PAYLOAD, CREATED_AT, UPDATED_AT)
        VALUES (:id, :source, :external_id, :origin_time, :latitude, :longitude, :depth_km,
          :magnitude, :magnitude_type, :place, :event_url, :provider_updated_at, :provider_status,
          :tsunami, :alert_level, :significance, :raw_payload, :created_at, :updated_at)
        """;

    private const string SelectColumns = """
        ID, SOURCE, EXTERNAL_ID, ORIGIN_TIME, LATITUDE, LONGITUDE, DEPTH_KM, MAGNITUDE,
        MAGNITUDE_TYPE, PLACE, EVENT_URL, PROVIDER_UPDATED_AT, PROVIDER_STATUS, TSUNAMI,
        ALERT_LEVEL, SIGNIFICANCE, RAW_PAYLOAD, CREATED_AT, UPDATED_AT
        """;

    public async Task UpsertAsync(Earthquake e, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new OracleCommand(UpsertSql, connection) { BindByName = true };
        AddParameters(command, e);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Earthquake?> GetBySourceIdentityAsync(
        string source, string externalId, CancellationToken cancellationToken = default)
    {
        const string sql = $"SELECT {SelectColumns} FROM EARTHQUAKES WHERE SOURCE=:source AND EXTERNAL_ID=:external_id";
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new OracleCommand(sql, connection) { BindByName = true };
        Add(command, "source", OracleDbType.Varchar2, source);
        Add(command, "external_id", OracleDbType.Varchar2, externalId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<Earthquake>> SearchAsync(
        EarthquakeQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Limit is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(query.Limit), "Limit must be between 1 and 1000.");
        if (query.From.HasValue && query.To.HasValue && query.To <= query.From)
            throw new ArgumentException("The end of the period must be later than its start.");

        const string sql = $"""
            SELECT {SelectColumns} FROM EARTHQUAKES
            WHERE (:from_time IS NULL OR ORIGIN_TIME >= :from_time)
              AND (:to_time IS NULL OR ORIGIN_TIME < :to_time)
              AND (:minimum_magnitude IS NULL OR MAGNITUDE >= :minimum_magnitude)
              AND (:maximum_magnitude IS NULL OR MAGNITUDE <= :maximum_magnitude)
              AND (:minimum_latitude IS NULL OR LATITUDE >= :minimum_latitude)
              AND (:maximum_latitude IS NULL OR LATITUDE <= :maximum_latitude)
              AND (:minimum_longitude IS NULL OR LONGITUDE >= :minimum_longitude)
              AND (:maximum_longitude IS NULL OR LONGITUDE <= :maximum_longitude)
            ORDER BY ORIGIN_TIME DESC
            FETCH FIRST :limit ROWS ONLY
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new OracleCommand(sql, connection) { BindByName = true };
        Add(command, "from_time", OracleDbType.TimeStampTZ, query.From?.UtcDateTime);
        Add(command, "to_time", OracleDbType.TimeStampTZ, query.To?.UtcDateTime);
        Add(command, "minimum_magnitude", OracleDbType.Decimal, query.MinimumMagnitude);
        Add(command, "maximum_magnitude", OracleDbType.Decimal, query.MaximumMagnitude);
        Add(command, "minimum_latitude", OracleDbType.Decimal, query.MinimumLatitude);
        Add(command, "maximum_latitude", OracleDbType.Decimal, query.MaximumLatitude);
        Add(command, "minimum_longitude", OracleDbType.Decimal, query.MinimumLongitude);
        Add(command, "maximum_longitude", OracleDbType.Decimal, query.MaximumLongitude);
        Add(command, "limit", OracleDbType.Int32, query.Limit);

        var results = new List<Earthquake>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(Map(reader));
        return results;
    }

    private static Earthquake Map(OracleDataReader reader) => Earthquake.FromPersistence(
        new Guid((byte[])reader[0]), reader.GetString(1), reader.GetString(2),
        new DateTimeOffset(reader.GetDateTime(3), TimeSpan.Zero), reader.GetDecimal(4), reader.GetDecimal(5),
        reader.IsDBNull(6) ? null : reader.GetDecimal(6), reader.IsDBNull(7) ? null : reader.GetDecimal(7),
        reader.IsDBNull(8) ? null : reader.GetString(8), reader.IsDBNull(9) ? null : reader.GetString(9),
        reader.IsDBNull(10) ? null : reader.GetString(10),
        reader.IsDBNull(11) ? null : new DateTimeOffset(reader.GetDateTime(11), TimeSpan.Zero),
        reader.IsDBNull(12) ? null : reader.GetString(12), reader.IsDBNull(13) ? null : reader.GetInt32(13),
        reader.IsDBNull(14) ? null : reader.GetString(14), reader.IsDBNull(15) ? null : reader.GetInt32(15),
        reader.IsDBNull(16) ? null : reader.GetString(16),
        new DateTimeOffset(reader.GetDateTime(17), TimeSpan.Zero),
        new DateTimeOffset(reader.GetDateTime(18), TimeSpan.Zero));

    private static void AddParameters(OracleCommand command, Earthquake e)
    {
        Add(command, "source", OracleDbType.Varchar2, e.Source);
        Add(command, "external_id", OracleDbType.Varchar2, e.ExternalId);
        Add(command, "id", OracleDbType.Raw, e.Id.ToByteArray());
        Add(command, "origin_time", OracleDbType.TimeStampTZ, e.OriginTime.UtcDateTime);
        Add(command, "latitude", OracleDbType.Decimal, e.Latitude);
        Add(command, "longitude", OracleDbType.Decimal, e.Longitude);
        Add(command, "depth_km", OracleDbType.Decimal, e.DepthKm);
        Add(command, "magnitude", OracleDbType.Decimal, e.Magnitude);
        Add(command, "magnitude_type", OracleDbType.Varchar2, e.MagnitudeType);
        Add(command, "place", OracleDbType.Varchar2, e.Place);
        Add(command, "event_url", OracleDbType.Varchar2, e.EventUrl);
        Add(command, "provider_updated_at", OracleDbType.TimeStampTZ, e.ProviderUpdatedAt?.UtcDateTime);
        Add(command, "provider_status", OracleDbType.Varchar2, e.ProviderStatus);
        Add(command, "tsunami", OracleDbType.Int32, e.Tsunami);
        Add(command, "alert_level", OracleDbType.Varchar2, e.AlertLevel);
        Add(command, "significance", OracleDbType.Int32, e.Significance);
        Add(command, "raw_payload", OracleDbType.Clob, e.RawPayload);
        Add(command, "created_at", OracleDbType.TimeStampTZ, e.CreatedAt.UtcDateTime);
        Add(command, "updated_at", OracleDbType.TimeStampTZ, e.UpdatedAt.UtcDateTime);
    }

    private static void Add(OracleCommand command, string name, OracleDbType type, object? value)
    {
        var parameter = command.Parameters.Add(name, type);
        parameter.Value = value ?? DBNull.Value;
    }
}
