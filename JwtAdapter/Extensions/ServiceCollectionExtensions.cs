using System.Security.Claims;
using System.Text;
using JwtAdapter.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
// ReSharper disable All

namespace JwtAdapter.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection EnableAuthentication<T>(
        this IServiceCollection services,
        Action<BearerTokenConfig> bearerTokenConfigAction,
        Func<string, Task<IdentityValidationResults<T>>> validationFunction,
        bool validateLifeTime = true)
    {

        if (bearerTokenConfigAction == null)
        {
            throw new ArgumentNullException(nameof(bearerTokenConfigAction));
        }
        if (validationFunction == null)
        {
            throw new ArgumentNullException(nameof(validationFunction));
        }

        var bearerConfig = new BearerTokenConfig();
        bearerTokenConfigAction.Invoke(bearerConfig);
        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {

                x.SaveToken = true;
                x.ClaimsIssuer = bearerConfig.Issuer;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = bearerConfig.Issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerConfig.SigningKey!)),
                    ValidAudience = bearerConfig.Audience,
                    ValidateAudience = true,
                    ValidateLifetime = validateLifeTime
                };
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        var id = ctx.Principal?.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                        if (string.IsNullOrWhiteSpace(id))
                        {
                            ctx.Fail("Invalid token: missing user identifier.");
                            return;
                        }

                        var results = await validationFunction(id);

                        if (!results.IsSuccessful)
                        {
                            ctx.Fail(results.Message ?? "Validation failed.");
                            return;
                        }

                        // Ensure Authorization header is valid
                        if (!ctx.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
                            !authHeader[0]!.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Fail("Invalid Authorization header.");
                            return;
                        }

                        var bearerAuth = authHeader[0]!.Substring("Bearer ".Length);

                        // Add custom claims
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Thumbprint, results.IdentityData?.ToJsonString() ?? string.Empty),
                            new Claim(ClaimTypes.Authentication, bearerAuth)
                        };

                        var appIdentity = new ClaimsIdentity(claims, "AuthData");
                        ctx.Principal?.AddIdentity(appIdentity);
                    }
                };
            });

        return services;
    }
}