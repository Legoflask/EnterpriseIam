using System;
using System.Collections.Generic;

namespace EnterpriseIam.Core.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Relational tracking item
    public ICollection<User> Users { get; set; } = new List<User>();
}