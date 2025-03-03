using APIWMS.Helpers;
using Microsoft.Extensions.Options;
using System.Text;

namespace APIWMS.Middleware
{
    public class BasicAuthHandler
    {
        private readonly RequestDelegate _next;
        private readonly string _relm;
        private readonly BasicAuthSettings _config;

        public BasicAuthHandler(RequestDelegate next, string relm, IOptions<BasicAuthSettings> config)
        {
            _next = next;
            _relm = relm;
            _config = config.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\": \"Unauthorized\"}");
                await context.Response.CompleteAsync();
                return;
            }

            var header = context.Request.Headers["Authorization"];
            var encodedCreds = header.ToString().Substring(6);

            try
            {
                var creds = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCreds));
                string[] uidpwd = creds.Split(':');

                if (uidpwd.Length != 2)
                {
                    throw new Exception("Invalid credentials format.");
                }

                var uid = uidpwd[0];
                var password = uidpwd[1];

                if (uid != _config.Username || password != _config.Password)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\": \"Unauthorized - Invalid credentials\"}");
                    await context.Response.CompleteAsync();
                    return;
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\": \"Invalid Authorization Header\"}");
                await context.Response.CompleteAsync();
                return;
            }

            await _next(context);
        }
    }
}
