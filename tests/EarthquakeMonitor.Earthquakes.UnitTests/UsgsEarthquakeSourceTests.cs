using System.Net;
using System.Net.Http.Json;
using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;
using EarthquakeMonitor.Earthquakes.Infrastructure.Sources.USGS;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EarthquakeMonitor.Earthquakes.UnitTests;

public sealed class UsgsEarthquakeSourceTests
{
    [Fact]
    public async Task GetEventsAsync_builds_query_and_maps_response()
    {
        var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://earthquake.usgs.gov/")
        };
        var configuration = new ConfigurationManager();
        configuration["USGS:QueryEndpoint"] = "fdsnws/event/1/query";

        var source = new UsgsEarthquakeSource(
            httpClient,
            configuration,
            new UsgsEarthquakeMapper());

        var result = await source.GetEventsAsync(new SourceQuery(
            DateTimeOffset.Parse("2026-08-20T00:00:00Z"),
            DateTimeOffset.Parse("2026-08-20T01:00:00Z"),
            MinimumMagnitude: 2.5m,
            Limit: 10));

        Assert.NotNull(handler.RequestUri);
        Assert.Contains("format=geojson", handler.RequestUri!.Query);
        Assert.Contains("minmagnitude=2.5", handler.RequestUri.Query);
        Assert.Single(result.Events);
        Assert.Equal("us-test-1", result.Events[0].ExternalId);
    }

    [Fact]
    public async Task GetEventsAsync_throws_for_non_success_response()
    {
        var handler = new RecordingHandler(HttpStatusCode.TooManyRequests);
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://earthquake.usgs.gov/") };
        var source = new UsgsEarthquakeSource(httpClient, new ConfigurationManager(), new UsgsEarthquakeMapper());

        await Assert.ThrowsAsync<HttpRequestException>(() => source.GetEventsAsync(new(
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow)));
    }

    private sealed class RecordingHandler(HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            var response = new HttpResponseMessage(statusCode) { RequestMessage = request };
            if (statusCode == HttpStatusCode.OK)
            {
                response.Content = JsonContent.Create(new
                {
                    type = "FeatureCollection",
                    features = new[]
                    {
                        new
                        {
                            id = "us-test-1",
                            properties = new { mag = 3.1, time = 1_700_000_000_000L, magType = "ml", type = "earthquake" },
                            geometry = new { type = "Point", coordinates = new[] { -84.08m, 9.93m, 10.0m } }
                        }
                    }
                });
            }
            return Task.FromResult(response);
        }
    }
}
