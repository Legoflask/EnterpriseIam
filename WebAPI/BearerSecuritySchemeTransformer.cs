using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseIam.WebApi;

internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // 1. Initialize the base metadata collection layers safely to prevent internal null exceptions
        document.Components ??= new OpenApiComponents();

        // CRITICAL PROTECTION LAYER: Initialize the inner dictionary map if it is null!
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        // 2. Explicitly define our Bearer security layout specifications
        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Please enter your JWT Token into this field."
        };

        // 3. Add the scheme blueprint to your document definitions securely
        document.Components.SecuritySchemes["Bearer"] = securityScheme;

        // 4. Safely loop through every discovered API endpoint path and inject the security requirement mapping
        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                var requirement = new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                };

                operation.Security.Add(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
