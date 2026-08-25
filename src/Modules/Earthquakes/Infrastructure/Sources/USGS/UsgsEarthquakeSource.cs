using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using EarthquakeMonitor.Earthquakes.Application.Abstractions.Sources;
using EarthquakeMonitor.Earthquakes.Infrastructure.Sources.USGS.Models;
using Microsoft.Extensions.Configuration;

namespace EarthquakeMonitor.Earthquakes.Infrastructure.Sources.USGS;

public sealed class UsgsEarthquakeSource(
    HttpClient httpClient,
    IConfiguration configuration,
    UsgsEarthquakeMapper mapper) : ISeismicSource
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SourceQueryResult> GetEventsAsync(
        SourceQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.EndTime <= query.StartTime)
            throw new ArgumentException("The end time must be later than the start time.", nameof(query));
        if (query.Offset < 1) throw new ArgumentOutOfRangeException(nameof(query.Offset));
        if (query.Limit is < 1 or > 20000) throw new ArgumentOutOfRangeException(nameof(query.Limit));

        var endpoint = configuration["USGS:QueryEndpoint"] ?? "fdsnws/event/1/query";
        var parameters = new Dictionary<string, string>
        {
            ["format"] = "geojson",
            ["starttime"] = query.StartTime.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
            ["endtime"] = query.EndTime.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
            ["orderby"] = "time-asc",
            ["offset"] = query.Offset.ToString(CultureInfo.InvariantCulture),
            ["limit"] = query.Limit.ToString(CultureInfo.InvariantCulture)
        };
        // USGS only supports updatedafter together with eventid. Sending it on
        // a general time-window query can cause the provider to return no data.
        AddOptional(parameters, "minmagnitude", query.MinimumMagnitude);
        AddOptional(parameters, "minlatitude", query.MinimumLatitude);
        AddOptional(parameters, "maxlatitude", query.MaximumLatitude);
        AddOptional(parameters, "minlongitude", query.MinimumLongitude);
        AddOptional(parameters, "maxlongitude", query.MaximumLongitude);

        var uri = endpoint + "?" + string.Join("&", parameters.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        using var response = await httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var document = await response.Content.ReadFromJsonAsync<UsgsFeatureCollection>(JsonOptions, cancellationToken)
            ?? throw new InvalidDataException("USGS returned an empty response.");

        var events = new List<ExternalEarthquake>(document.Features.Count);
        foreach (var feature in document.Features)
            events.Add(mapper.Map(feature));

        return new SourceQueryResult(
            events,
            document.Features.Count >= query.Limit,
            document.Features.Count >= query.Limit
                ? (query.Offset + query.Limit).ToString(CultureInfo.InvariantCulture)
                : null);
    }

    private static void AddOptional(Dictionary<string, string> parameters, string key, object? value)
    {
        if (value is null) return;
        parameters[key] = value switch
        {
            DateTimeOffset date => date.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture)!,
            _ => value.ToString()!
        };
    }
}
