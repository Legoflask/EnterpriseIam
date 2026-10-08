using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EnterpriseIam.Core.Entities;
using EnterpriseIam.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIam.WebApi.Controllers;

[Authorize] // Enforces valid JWT token presence
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public DocumentsController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file payload provided.");
        }

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF document configurations are accepted.");
        }

        // 1. Generate the tracking row signature
        var trackingId = Guid.NewGuid();
        var newDocument = new Document
        {
            Id = trackingId,
            FileName = file.FileName,
            Status = "Processing"
        };

        // Note: Our DbContext interceptor will automatically populate the TenantId column securely!
        _context.Documents.Add(newDocument);
        await _context.SaveChangesAsync();

        // 2. Stream the file bytes and forward them asynchronously to the Python worker
        try
        {
            using var client = _httpClientFactory.CreateClient();
            using var content = new MultipartFormDataContent();

            // Allocate the Document Tracker reference ID form field
            content.Add(new StringContent(trackingId.ToString()), "document_id");

            // Attach the raw file data binary stream block
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", file.FileName);

            // 🚀 Fire HTTP Proxy request call straight over to your Python FastAPI service worker container
            var workerUrl = "http://localhost:8000/api/documents/extract";
            var response = await client.PostAsync(workerUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                newDocument.Status = "Failed";
                await _context.SaveChangesAsync();
                return StatusCode((int)response.StatusCode, "Python parsing microservice worker reported an internal extraction crash.");
            }

            return Ok(new DocumentUploadResult(trackingId, "Processing", "File received and processing initiated across distributed microservice node."));
        }
        catch (Exception ex)
        {
            newDocument.Status = "Failed";
            await _context.SaveChangesAsync();
            return StatusCode(500, $"API gateway failed to link up with background workers: {ex.Message}");
        }
    }
}