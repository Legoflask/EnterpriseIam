using System;
using System.Collections.Generic;
using System.Data;

namespace EnterpriseIam.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; } // Enforces the database multi-tenant anchor partition
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation Properties (EF Core uses these to map SQL relational joins)
    public Tenant? Tenant { get; set; }
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}