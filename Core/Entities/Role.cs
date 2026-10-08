using System;
using System.Collections.Generic;

namespace EnterpriseIam.Core.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Admin", "User", "DocumentAuditor"

    // Navigation Property for the Many-to-Many connection
    public ICollection<User> Users { get; set; } = new List<User>();
}