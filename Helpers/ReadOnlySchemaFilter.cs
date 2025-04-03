using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Annotations;

public class ReadOnlySchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null || context?.Type == null)
            return;

        var readOnlyProperties = context.Type.GetProperties()
            .Where(prop => prop.GetCustomAttribute<SwaggerSchemaAttribute>()?.ReadOnly == true)
            .Select(prop => prop.Name);

        foreach (var prop in readOnlyProperties)
        {
            if (schema.Properties.ContainsKey(prop))
            {
                schema.Properties[prop].ReadOnly = true;
            }
        }
    }
}
