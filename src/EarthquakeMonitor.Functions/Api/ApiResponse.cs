using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using EarthquakeMonitor.Functions.Security;

namespace EarthquakeMonitor.Functions.Api;

public static class ApiResponse
{
    public static async Task<HttpResponseData?> RejectIfUnauthorizedAsync(
        HttpRequestData request, SecurityDecision decision, CancellationToken cancellationToken)
    {
        if (decision == SecurityDecision.Allowed)
            return null;

        var response = request.CreateResponse(decision switch
        {
            SecurityDecision.ConfigurationError => HttpStatusCode.ServiceUnavailable,
            SecurityDecision.RateLimited => HttpStatusCode.TooManyRequests,
            _ => HttpStatusCode.Unauthorized
        });
        response.Headers.Add("Cache-Control", "no-store");
        if (decision == SecurityDecision.RateLimited)
            response.Headers.Add("Retry-After", "60");
        await response.WriteAsJsonAsync(new
        {
            error = decision == SecurityDecision.RateLimited ? "rate_limit_exceeded" : "unauthorized"
        }, cancellationToken);
        return response;
    }

    public static void AddSecurityHeaders(HttpResponseData response) =>
        response.Headers.Add("X-Content-Type-Options", "nosniff");
}
