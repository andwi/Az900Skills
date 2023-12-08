using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;

const string SkillsBlobContainerName = "skills";
const string SkillsBlobName = "skills.json";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAzureClients(clients =>
{
    if (Uri.TryCreate(builder.Configuration["BlobServiceUri"], UriKind.Absolute, out Uri? serviceUri))
    {
        clients.AddBlobServiceClient(serviceUri);
    }
    else if (builder.Configuration["BlobConnectionString"] is string connectionString)
    {
        clients.AddBlobServiceClient(connectionString);
    }

    clients.UseCredential(new DefaultAzureCredential());
});

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/skills", async (ILogger<Program> logger, HttpContext httpContext) =>
{
    try
    {
        if (httpContext.RequestServices.GetService<BlobServiceClient>() is BlobServiceClient blobServiceClient)
        {
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(SkillsBlobContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(SkillsBlobName);

            BlobDownloadStreamingResult streamingResult = await blobClient.DownloadStreamingAsync();
            return Results.File(streamingResult.Content, streamingResult.Details.ContentType);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Error fetching skills from blob storage");
    }

    string path = Path.Combine(builder.Environment.ContentRootPath, SkillsBlobName);
    return Results.File(path, contentType: "application/json");
});

app.MapPost("/skills", async (HttpContext httpContext) =>
{
    BlobServiceClient blobServiceClient = httpContext.RequestServices.GetRequiredService<BlobServiceClient>();
    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(SkillsBlobContainerName);
    BlobClient blobClient = containerClient.GetBlobClient(SkillsBlobName);

    await blobClient.UploadAsync(httpContext.Request.Body, new BlobUploadOptions
    {
        HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" }
    });
});

app.Run();
