using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CLDV7112_Functions;

public class BlobImageAuditFunction
{
    private readonly ILogger<BlobImageAuditFunction> _logger;

    public BlobImageAuditFunction(ILogger<BlobImageAuditFunction> logger)
    {
        _logger = logger;
    }

    [Function("BlobImageAudit")]
    public async Task Run(
        [BlobTrigger("drone-images/{name}", Connection = "AzureWebJobsStorage")] BlobClient blobClient,
        string name)
    {
        var props = await blobClient.GetPropertiesAsync();

        var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var table = new TableClient(conn, "ImageAudit");
        await table.CreateIfNotExistsAsync();

        var entity = new TableEntity("DroneImages", Guid.NewGuid().ToString())
        {
            { "BlobName", name },
            { "SizeBytes", props.Value.ContentLength },
            { "ContentType", props.Value.ContentType },
            { "Status", "Processed" },
            { "ProcessedUtc", DateTime.UtcNow }
        };

        await table.AddEntityAsync(entity);

        _logger.LogInformation("Audit entry written for image {name}", name);
    }
}