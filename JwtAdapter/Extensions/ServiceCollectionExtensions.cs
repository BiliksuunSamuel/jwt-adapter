using System.Security.Claims;
using System.Text;
using JwtAdapter.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
                x.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = async (ctx) =>
                    {
                        var id = ctx.Principal!.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                        var results = await validationFunction(id!);
                        
                        var bearerAuth = ctx.HttpContext.Request.Headers["Authorization"][0]!.Split(new[] { ' ' })[1];

                        if (!results.IsSuccessful)
                        {
                            ctx.Fail(results.Message ?? "Validation failed");
                            return;
                        }

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Thumbprint, results.IdentityData!.ToJsonString()),
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