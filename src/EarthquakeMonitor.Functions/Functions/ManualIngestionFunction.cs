using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;
using EarthquakeMonitor.Earthquakes.Application.UseCases.Ingestion;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace EarthquakeMonitor.Functions.Functions;

public sealed class ManualIngestionFunction(IIngestEarthquakesUseCase useCase, IConfiguration configuration)
{
    // Admin authorization requires the Functions host master key. This route is not part of the public API.
    [Function("ManualIngestEarthquakes")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "management/ingestion")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        var end = DateTimeOffset.UtcNow;
        var lookback = configuration.GetValue<int>("USGS:InitialLookbackHours");
        var overlap = configuration.GetValue<int>("USGS:OverlapMinutes");
        var result = await useCase.ExecuteAsync(new SourceQuery(end.AddHours(-lookback).AddMinutes(-overlap), end, end.AddHours(-lookback), configuration.GetValue<int>("USGS:PageSize")), cancellationToken);
        var response = request.CreateResponse(result.FailedCount == 0 ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
        await response.WriteAsJsonAsync(result, cancellationToken);
        return response;
    }
}
