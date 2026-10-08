using System;

namespace EnterpriseIam.Core.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; } // Keeps files isolated per company
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation item
    public Tenant? Tenant { get; set; }
}