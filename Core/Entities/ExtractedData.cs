using System;

namespace EnterpriseIam.Core.Entities;

public class ExtractedData
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string RawText { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}