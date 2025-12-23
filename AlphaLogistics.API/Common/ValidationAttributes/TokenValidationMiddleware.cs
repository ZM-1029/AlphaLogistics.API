using System.Security.Claims;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace WALMS.API.Common.ValidationAttributes
{
    // Middleware for validating JWT tokens
    public class TokenValidationMiddleware
    {
        // Field to hold the reference to the next middleware in the pipeline
        private readonly RequestDelegate _next;

        // Field to hold the logger instance for logging within the middleware
        private readonly ILogger<TokenValidationMiddleware> _logger;

        // Field to hold the configuration settings required for token validation
        private readonly IConfiguration _configuration;


        // Constructor to initialize middleware dependencies
        public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        // Method to return an invalid token response
        private async Task ReturnInvalidTokenResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(message);
        }

        // Method called for each HTTP request
        /*public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
                await ValidateTokenAsync(token, context);
            else if (token == null)
                await _next.Invoke(context);
            else
                await ReturnInvalidTokenResponse(context, "Token is missing");
        }*/

        // Method to validate the JWT token
    /*    private async Task ValidateTokenAsync(string token, HttpContext context)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    await ReturnInvalidTokenResponse(context, "Invalid token");
                    return;
                }
                else if (validatedToken.ValidTo < DateTime.UtcNow)
                {
                    // Token is expired
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token has expired.");
                    return;
                }
                else
                {
                    var username = (validatedToken as JwtSecurityToken).Claims.First(x => x.Type == ClaimTypes.Name).Value;
                    var userRoles = jwtSecurityToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

                    // Store in HttpContext.Items
                    context.Items["Username"] = username;
                    context.Items["UserRoles"] = userRoles;

                    // Redirect based on user roles
                    string redirectUrl = GetRedirectUrlForUser(userRoles);

                    // Send redirect response
                    context.Response.Redirect(redirectUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed");
                await ReturnInvalidTokenResponse(context, "Token validation failed");
            }
            await _next.Invoke(context);
        }

        // Method to determine the redirect URL based on user roles
        private string GetRedirectUrlForUser(List<string> userRoles)
        {
            // Logic to determine the redirect URL based on user roles
            if (userRoles.Contains("WarehouseRole"))
            {
                return "https://warehouse.example.com/home";
            }
            else if (userRoles.Contains("InventoryRole"))
            {
                return "https://inventory.example.com/home";
            }
            // Add more cases for other roles as needed
            else
            {
                // Default redirect URL if no specific role-based URL is configured
                return "https://default.example.com/home";
            }
        }*/
    }
}
