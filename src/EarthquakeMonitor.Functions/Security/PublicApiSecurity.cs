using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

namespace EarthquakeMonitor.Functions.Security;

public sealed class PublicApiSecurity(IConfiguration configuration)
{
    private static readonly ConcurrentDictionary<string, Window> Windows = new();
    private readonly string _apiKey = configuration["PublicApi:Key"] ?? string.Empty;
    private readonly int _limit = Math.Clamp(configuration.GetValue("PublicApi:RequestsPerMinute", 60), 1, 600);
    private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

    public SecurityDecision Authorize(HttpRequestData request)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return SecurityDecision.ConfigurationError;

        if (!request.Headers.TryGetValues("x-api-key", out var values))
            return SecurityDecision.Unauthorized;

        var supplied = values.FirstOrDefault() ?? string.Empty;
        var expected = Encoding.UTF8.GetBytes(_apiKey);
        var actual = Encoding.UTF8.GetBytes(supplied);
        if (expected.Length != actual.Length || !CryptographicOperations.FixedTimeEquals(expected, actual))
            return SecurityDecision.Unauthorized;

        var clientKey = GetClientKey(request);
        var now = DateTimeOffset.UtcNow;
        var window = Windows.GetOrAdd(clientKey, _ => new Window(now));
        lock (window)
        {
            if (now - window.Start >= _window)
            {
                window.Start = now;
                window.Count = 0;
            }

            window.Count++;
            return window.Count > _limit
                ? SecurityDecision.RateLimited
                : SecurityDecision.Allowed;
        }
    }

    private static string GetClientKey(HttpRequestData request)
    {
        if (request.Headers.TryGetValues("x-forwarded-for", out var forwarded))
            return forwarded.FirstOrDefault()?.Split(',')[0].Trim() ?? "unknown";
        return request.Headers.TryGetValues("x-client-ip", out var client)
            ? client.FirstOrDefault() ?? "unknown"
            : "unknown";
    }

    private sealed class Window(DateTimeOffset start)
    {
        public DateTimeOffset Start { get; set; } = start;
        public int Count { get; set; }
    }
}

public enum SecurityDecision
{
    Allowed,
    ConfigurationError,
    Unauthorized,
    RateLimited
}
