using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace APIWMS.Helpers
{
    public class CustomTagOrderFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var orderedTags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = "Documents", Description = "Endpoints for managing documents" },
                new OpenApiTag { Name = "Products", Description = "Endpoints for managing products" },
                new OpenApiTag { Name = "Logs", Description = "Endpoints for logs" }
            };

            swaggerDoc.Tags = orderedTags;
        }
    }
}
