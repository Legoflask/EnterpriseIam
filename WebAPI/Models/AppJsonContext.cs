using System.Text.Json.Serialization;
using EnterpriseIam.WebApi.Models;
using System.Collections.Generic;

namespace EnterpriseIam.WebApi;

// 🚀 Instructs .NET 10 to generate optimized metadata for your request shapes at compile time
[JsonSerializable(typeof(RegisterTenantRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(AuthResponse))]
[JsonSerializable(typeof(List<UserDto>))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(DocumentUploadResult))]
public partial class AppJsonContext : JsonSerializerContext
{
}

public record DocumentUploadResult(Guid DocumentId, string Status, string Message);