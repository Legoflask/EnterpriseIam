using System.ComponentModel.DataAnnotations;

namespace EnterpriseIam.WebApi.Models;

public record RegisterTenantRequest(
    [Required][MaxLength(250)] string TenantName,
    [Required][EmailAddress][MaxLength(256)] string AdminEmail,
    [Required][MinLength(8)] string Password
);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(
    string AccessToken,
    string Email,
    Guid TenantId
);

public record UserDto(
    Guid Id, 
    string Email, 
    DateTime CreatedAtUtc
);