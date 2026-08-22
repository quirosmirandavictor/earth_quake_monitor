using System.Globalization;
using System.Net;
using EarthquakeMonitor.Analytics.Application.Abstractions;
using EarthquakeMonitor.Earthquakes.Application.Abstractions.Persistence;
using EarthquakeMonitor.Earthquakes.Domain;
using EarthquakeMonitor.Functions.Api;
using EarthquakeMonitor.Functions.Security;
using EarthquakeMonitor.Regions.Application.Abstractions;
using EarthquakeMonitor.Regions.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace EarthquakeMonitor.Functions.Functions;

public sealed class EarthquakeQueryFunctions(
    IEarthquakeRepository earthquakes,
    IAnalyticsRepository analytics,
    IRegionCatalog regions,
    PublicApiSecurity security,
    ILogger<EarthquakeQueryFunctions> logger)
{
    [Function("SearchEarthquakes")]
    public async Task<HttpResponseData> Search(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/earthquakes")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        var rejection = await ApiResponse.RejectIfUnauthorizedAsync(request, security.Authorize(request), cancellationToken);
        if (rejection is not null) return rejection;

        try
        {
            var query = ParseQuery(request);
            var result = await earthquakes.SearchAsync(query.Query, cancellationToken);
            var response = request.CreateResponse(HttpStatusCode.OK);
            ApiResponse.AddSecurityHeaders(response);
            await response.WriteAsJsonAsync(new { items = result.Select(ToDto), count = result.Count, limit = query.Query.Limit }, cancellationToken);
            logger.LogInformation("Earthquake query completed. Count={Count}, Region={Region}.", result.Count, query.Region);
            return response;
        }
        catch (ApiValidationException exception)
        {
            return await Error(request, HttpStatusCode.BadRequest, "invalid_query", exception.Message, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Earthquake query failed.");
            return await Error(request, HttpStatusCode.InternalServerError, "query_failed", "The query could not be completed.", cancellationToken);
        }
    }

    [Function("GetEarthquake")]
    public async Task<HttpResponseData> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/earthquakes/{source}/{externalId}")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        var rejection = await ApiResponse.RejectIfUnauthorizedAsync(request, security.Authorize(request), cancellationToken);
        if (rejection is not null) return rejection;

        var binding = request.FunctionContext.BindingContext.BindingData;
        var source = binding.TryGetValue("source", out var sourceValue) ? sourceValue?.ToString() : null;
        var externalId = binding.TryGetValue("externalId", out var idValue) ? idValue?.ToString() : null;
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(externalId))
            return await Error(request, HttpStatusCode.BadRequest, "invalid_identity", "Source and externalId are required.", cancellationToken);

        var result = await earthquakes.GetBySourceIdentityAsync(source, externalId, cancellationToken);
        if (result is null)
            return await Error(request, HttpStatusCode.NotFound, "not_found", "Earthquake not found.", cancellationToken);

        var response = request.CreateResponse(HttpStatusCode.OK);
        ApiResponse.AddSecurityHeaders(response);
        await response.WriteAsJsonAsync(ToDto(result), cancellationToken);
        return response;
    }

    [Function("GetEarthquakeAnalytics")]
    public async Task<HttpResponseData> Analytics(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/analytics/earthquakes")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        var rejection = await ApiResponse.RejectIfUnauthorizedAsync(request, security.Authorize(request), cancellationToken);
        if (rejection is not null) return rejection;
        try
        {
            var query = QueryHelpers.ParseQuery(request.Url.Query);
            var (from, to) = ParsePeriod(query);
            var result = await analytics.GetSummaryAsync(from, to, cancellationToken);
            var response = request.CreateResponse(HttpStatusCode.OK);
            ApiResponse.AddSecurityHeaders(response);
            await response.WriteAsJsonAsync(result, cancellationToken);
            return response;
        }
        catch (ApiValidationException exception)
        {
            return await Error(request, HttpStatusCode.BadRequest, "invalid_query", exception.Message, cancellationToken);
        }
    }

    private (EarthquakeQuery Query, string Region) ParseQuery(HttpRequestData request)
    {
        var values = QueryHelpers.ParseQuery(request.Url.Query);
        var (from, to) = ParsePeriod(values, allowMissing: true);
        var regionText = Get(values, "region") ?? "Global";
        if (!Enum.TryParse<RegionCode>(regionText, true, out var regionCode))
            throw new ApiValidationException("region is invalid.");
        var bounds = regions.Get(regionCode).Bounds;
        var query = new EarthquakeQuery(from, to,
            Decimal(values, "minMagnitude"), Decimal(values, "maxMagnitude"),
            Max(Decimal(values, "minLatitude"), bounds?.MinimumLatitude),
            Min(Decimal(values, "maxLatitude"), bounds?.MaximumLatitude),
            Max(Decimal(values, "minLongitude"), bounds?.MinimumLongitude),
            Min(Decimal(values, "maxLongitude"), bounds?.MaximumLongitude),
            Int(values, "limit") ?? 100);
        Validate(query);
        return (query, regionCode.ToString());
    }

    private static void Validate(EarthquakeQuery query)
    {
        if (query.Limit is < 1 or > 100) throw new ApiValidationException("limit must be between 1 and 100.");
        if (query.From.HasValue && query.To.HasValue && query.To <= query.From) throw new ApiValidationException("to must be later than from.");
        if (query.MinimumMagnitude > query.MaximumMagnitude) throw new ApiValidationException("minMagnitude cannot exceed maxMagnitude.");
        if (query.MinimumLatitude > query.MaximumLatitude || query.MinimumLongitude > query.MaximumLongitude)
            throw new ApiValidationException("The geographic bounds do not overlap.");
    }

    private static (DateTimeOffset From, DateTimeOffset To) ParsePeriod(
        IDictionary<string, Microsoft.Extensions.Primitives.StringValues> values, bool allowMissing = false)
    {
        var fromText = Get(values, "from");
        var toText = Get(values, "to");
        if (allowMissing && fromText is null && toText is null)
            return (DateTimeOffset.UtcNow.AddDays(-7), DateTimeOffset.UtcNow);
        if (!DateTimeOffset.TryParse(fromText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var from) ||
            !DateTimeOffset.TryParse(toText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var to))
            throw new ApiValidationException("from and to must be ISO-8601 timestamps.");
        if (to <= from) throw new ApiValidationException("to must be later than from.");
        if (to - from > TimeSpan.FromDays(366)) throw new ApiValidationException("The maximum period is 366 days.");
        return (from, to);
    }

    private static string? Get(IDictionary<string, Microsoft.Extensions.Primitives.StringValues> values, string key) =>
        values.TryGetValue(key, out var value) ? value.FirstOrDefault() : null;
    private static decimal? Decimal(IDictionary<string, Microsoft.Extensions.Primitives.StringValues> v, string k) =>
        Get(v, k) is { } x && decimal.TryParse(x, NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : Get(v, k) is null ? null : throw new ApiValidationException($"{k} is invalid.");
    private static int? Int(IDictionary<string, Microsoft.Extensions.Primitives.StringValues> v, string k) =>
        Get(v, k) is { } x && int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : Get(v, k) is null ? null : throw new ApiValidationException($"{k} is invalid.");
    private static decimal? Max(decimal? value, decimal? bound) => value.HasValue && bound.HasValue ? Math.Max(value.Value, bound.Value) : value ?? bound;
    private static decimal? Min(decimal? value, decimal? bound) => value.HasValue && bound.HasValue ? Math.Min(value.Value, bound.Value) : value ?? bound;
    private static object ToDto(Earthquake e) => new
    {
        id = e.Id, source = e.Source, externalId = e.ExternalId, originTime = e.OriginTime,
        latitude = e.Latitude, longitude = e.Longitude, depthKm = e.DepthKm, magnitude = e.Magnitude,
        magnitudeType = e.MagnitudeType, place = e.Place, eventUrl = e.EventUrl,
        providerUpdatedAt = e.ProviderUpdatedAt, providerStatus = e.ProviderStatus,
        tsunami = e.Tsunami, alertLevel = e.AlertLevel, significance = e.Significance
    };
    private static async Task<HttpResponseData> Error(HttpRequestData request, HttpStatusCode status, string code, string message, CancellationToken token)
    { var response = request.CreateResponse(status); ApiResponse.AddSecurityHeaders(response); await response.WriteAsJsonAsync(new { error = code, message }, token); return response; }
    private sealed class ApiValidationException(string message) : Exception(message);
}
