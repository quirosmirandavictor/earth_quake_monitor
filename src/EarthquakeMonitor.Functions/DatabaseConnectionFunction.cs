using EarthquakeMonitor.Earthquakes.Infrastructure.Persistence.Oracle;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace EarthquakeMonitor.Functions;

public sealed class DatabaseConnectionFunction(
    OracleConnectionFactory connectionFactory,
    ILogger<DatabaseConnectionFunction> logger)
{
    [Function("DatabaseConnection")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/database")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        var response = request.CreateResponse();
        try
        {
            await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            response.StatusCode = HttpStatusCode.OK;
            await response.WriteAsJsonAsync(new { status = "ok", database = "oracle" }, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Oracle database connection test failed.");
            response.StatusCode = HttpStatusCode.ServiceUnavailable;
            await response.WriteAsJsonAsync(new { status = "unavailable", database = "oracle" }, cancellationToken);
        }

        return response;
    }
}
