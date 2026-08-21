using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EarthquakeMonitor.Earthquakes.Domain;
using EarthquakeMonitor.Earthquakes.Infrastructure.Persistence.Oracle;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EarthquakeMonitor.Earthquakes.IntegrationTests;

public sealed class OracleEarthquakeRepositoryTests
{
    [Fact]
    public async Task Upsert_get_by_identity_and_search_work_against_oracle()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("ORACLE_INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
            return;

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Oracle")
            ?? throw new InvalidOperationException("ConnectionStrings__Oracle is required.");
        var configuration = new ConfigurationManager();
        configuration["ConnectionStrings:Oracle"] = connectionString;
        var repository = new OracleEarthquakeRepository(new OracleConnectionFactory(configuration));
        var externalId = $"integration-{Guid.NewGuid():N}";
        var earthquake = Earthquake.Create("INTEGRATION_TEST", externalId, DateTimeOffset.UtcNow,
            9.93m, -84.08m, magnitude: 4.2m, place: "Integration test");

        await repository.UpsertAsync(earthquake);
        var found = await repository.GetBySourceIdentityAsync(earthquake.Source, externalId);
        var results = await repository.SearchAsync(new(From: earthquake.OriginTime.AddMinutes(-1), To: earthquake.OriginTime.AddMinutes(1)));

        Assert.NotNull(found);
        Assert.Equal(externalId, found!.ExternalId);
        Assert.Contains(results, item => item.ExternalId == externalId);
    }
}
