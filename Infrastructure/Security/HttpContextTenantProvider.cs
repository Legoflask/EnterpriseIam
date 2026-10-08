using System;
using System.Security.Claims;
using EnterpriseIam.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EnterpriseIam.Infrastructure.Security;

public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");

            if (string.IsNullOrEmpty(claimValue) || !Guid.TryParse(claimValue, out var tenantGuid))
            {
                return Guid.Empty;
            }

            return tenantGuid;
        }
    }
}