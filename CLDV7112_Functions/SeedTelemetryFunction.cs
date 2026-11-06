using System.Net;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CLDV7112_Functions;

public class SeedTelemetryFunction
{
    [Function("SeedTelemetry")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var table = new TableClient(conn, "Telemetry");
        await table.CreateIfNotExistsAsync();

        var devices = new[] { "SENSOR-001", "SENSOR-002", "SENSOR-003" };
        var rnd = new Random();
        var now = DateTime.UtcNow;
        int count = 0;

        foreach (var device in devices)
        {
            for (int i = 0; i < 20; i++)
            {
                var readingTime = now.AddMinutes(-i * 5);
                var entity = new TableEntity(device, (DateTime.MaxValue.Ticks - readingTime.Ticks).ToString("D19"))
                {
                    { "SoilMoisture", Math.Round(20 + rnd.NextDouble() * 40, 1) },
                    { "Temperature", Math.Round(15 + rnd.NextDouble() * 20, 1) },
                    { "ReadingUtc", readingTime }
                };
                await table.AddEntityAsync(entity);
                count++;
            }
        }

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteStringAsync($"Seeded {count} readings.");
        return res;
    }
}