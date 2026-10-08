using System;

namespace EnterpriseIam.Core.Interfaces;

public interface ITenantProvider
{
    Guid TenantId { get; }
}