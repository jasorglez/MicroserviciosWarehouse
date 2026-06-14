using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace Warehouse.Middleware
{
    public class ApiLogForwarderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLogForwarderMiddleware> _logger;
        private readonly string? _securityBaseUrl;
        private const string MicroservicioName = "warehouse";

        private static readonly HashSet<string> _skip = new(StringComparer.OrdinalIgnoreCase)
            { "/swagger", "/health", "/favicon.ico", "/metrics" };

        public ApiLogForwarderMiddleware(RequestDelegate next,
            ILogger<ApiLogForwarderMiddleware> logger, IConfiguration config)
        {
            _next = next;
            _logger = logger;
            _securityBaseUrl = config["SecurityApi:BaseUrl"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            if (_skip.Any(s => path.StartsWith(s, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            var sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();

            _ = ForwardLogAsync(context, path, sw.ElapsedMilliseconds,
                context.Response.StatusCode);
        }

        private async Task ForwardLogAsync(HttpContext ctx, string path,
            long durationMs, int statusCode)
        {
            if (string.IsNullOrEmpty(_securityBaseUrl)) return;
            try
            {
                int? idUser = null;
                string? email = null;
                var auth = ctx.Request.Headers["Authorization"].FirstOrDefault();
                if (auth?.StartsWith("Bearer ") == true)
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var token = auth["Bearer ".Length..];
                        if (handler.CanReadToken(token))
                        {
                            var jwt = handler.ReadJwtToken(token);
                            var id = jwt.Claims.FirstOrDefault(c =>
                                c.Type == "nameid" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                            var em = jwt.Claims.FirstOrDefault(c =>
                                c.Type == "unique_name" || c.Type == System.Security.Claims.ClaimTypes.Name);
                            if (id != null && int.TryParse(id.Value, out var uid)) idUser = uid;
                            email = em?.Value;
                        }
                    }
                    catch { }
                }

                var ip = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
                         ?? ctx.Connection.RemoteIpAddress?.ToString();

                var ua = ctx.Request.Headers["User-Agent"].FirstOrDefault();

                var entry = new
                {
                    fechaHora     = DateTime.UtcNow,
                    endpoint      = path,
                    method        = ctx.Request.Method,
                    statusCode,
                    durationMs,
                    idUser,
                    userEmail     = email,
                    idCompany     = (int?)null,
                    microservicio = MicroservicioName,
                    ipAddress     = ip,
                    userAgent     = ua?[..Math.Min(500, ua.Length)]
                };

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                var json = new StringContent(
                    JsonSerializer.Serialize(new[] { entry }),
                    Encoding.UTF8, "application/json");
                await client.PostAsync($"{_securityBaseUrl}/api/ApiLog/ingest", json);
            }
            catch { /* nunca bloquear */ }
        }
    }
}
