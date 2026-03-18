using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MerchantDeviceManager.Web.Api;

/// <summary>
/// Adds X-Tenant-Id and X-Role headers to Swagger UI for API testing.
/// </summary>
public class SwaggerTenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-Id",
            In = ParameterLocation.Header,
            Description = "Merchant GUID (required for device endpoints). Example: 11111111-1111-1111-1111-111111111111",
            Required = false,
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Role",
            In = ParameterLocation.Header,
            Description = "Operator role: Admin, Support, or Viewer (required for POST /devices)",
            Required = false,
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
