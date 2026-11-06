using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CLDV7112_Functions;

public class ProcessIrrigationCommandFunction
{
    private readonly ILogger<ProcessIrrigationCommandFunction> _logger;

    public ProcessIrrigationCommandFunction(ILogger<ProcessIrrigationCommandFunction> logger)
    {
        _logger = logger;
    }

    [Function("ProcessIrrigationCommand")]
    public async Task Run(
        [QueueTrigger("irrigation-commands", Connection = "AzureWebJobsStorage")] string message)
    {
        var cmd = JsonSerializer.Deserialize<IrrigationCommand>(message,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (cmd == null || string.IsNullOrWhiteSpace(cmd.DeviceId))
        {
            _logger.LogWarning("Invalid irrigation command received: {message}", message);
            return;
        }

        var newStatus = cmd.Command == "StopIrrigation" ? "Idle" : "Irrigating";

        var connString = Environment.GetEnvironmentVariable("SqlConnection");

        await using var conn = new SqlConnection(connString);
        await conn.OpenAsync();

        await using var sql = new SqlCommand(
            "UPDATE Devices SET Status = @status, LastUpdated = SYSUTCDATETIME() WHERE DeviceId = @id",
            conn);
        sql.Parameters.AddWithValue("@status", newStatus);
        sql.Parameters.AddWithValue("@id", cmd.DeviceId);

        var rows = await sql.ExecuteNonQueryAsync();

        if (rows == 0)
            _logger.LogWarning("Device {id} not found in registry.", cmd.DeviceId);
        else
            _logger.LogInformation("Device {id} status set to {status}.", cmd.DeviceId, newStatus);
    }
}