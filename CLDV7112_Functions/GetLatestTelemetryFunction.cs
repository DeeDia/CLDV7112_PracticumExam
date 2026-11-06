using System.Net;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CLDV7112_Functions;

public class TelemetryRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public int Count { get; set; } = 10;
}

public class GetLatestTelemetryFunction
{
    [Function("GetLatestTelemetry")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var body = await req.ReadFromJsonAsync<TelemetryRequest>();

        if (body == null || string.IsNullOrWhiteSpace(body.DeviceId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("deviceId is required.");
            return bad;
        }

        var count = Math.Clamp(body.Count, 1, 50);

        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var table = new TableClient(conn, "Telemetry");

        var readings = new List<object>();
        var filter = TableClient.CreateQueryFilter($"PartitionKey eq {body.DeviceId}");

        await foreach (var e in table.QueryAsync<TableEntity>(filter: filter, maxPerPage: count))
        {
            readings.Add(new
            {
                DeviceId = e.PartitionKey,
                SoilMoisture = e.GetDouble("SoilMoisture"),
                Temperature = e.GetDouble("Temperature"),
                ReadingUtc = e.GetDateTimeOffset("ReadingUtc")
            });
            if (readings.Count >= count) break;
        }

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(readings);
        return res;
    }
}