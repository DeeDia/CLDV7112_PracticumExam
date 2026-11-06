using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CLDV7112_Functions;

public class SendIrrigationCommandFunction
{
    [Function("SendIrrigationCommand")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var cmd = await req.ReadFromJsonAsync<IrrigationCommand>();

        if (cmd == null || string.IsNullOrWhiteSpace(cmd.DeviceId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("deviceId is required.");
            return bad;
        }

        cmd.DurationMinutes = Math.Clamp(cmd.DurationMinutes, 1, 240);
        cmd.IssuedUtc = DateTime.UtcNow;

        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var queue = new QueueClient(conn, "irrigation-commands",
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        await queue.CreateIfNotExistsAsync();

        await queue.SendMessageAsync(JsonSerializer.Serialize(cmd));

        var res = req.CreateResponse(HttpStatusCode.Accepted);
        await res.WriteStringAsync($"Irrigation command queued for {cmd.DeviceId}.");
        return res;
    }
}