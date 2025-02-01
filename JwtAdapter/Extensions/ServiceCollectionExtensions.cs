using System.IdentityModel.Tokens.Jwt;
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
    
    /// <summary>
    /// Enable authentication for the application
    /// </summary>
    /// <param name="services"></param>
    /// <param name="bearerTokenConfigAction"></param>
    /// <param name="validationFunction"></param>
    /// <param name="validateLifeTime"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
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
    
    /// <summary>
    /// Generate SignIn Token For Application
    /// </summary>
    /// <param name="bearerTokenConfig"></param>
    /// <param name="identityId"></param>
    /// <param name="identityData"></param>
    /// <param name="expires"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<string> GenerateToken<T>(this T identityData, BearerTokenConfig bearerTokenConfig, string identityId,DateTime?expires)
    {
        var symmetricKey = Encoding.ASCII.GetBytes(bearerTokenConfig.SigningKey!);
        var now = DateTime.UtcNow;


        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, identityId),
            new Claim(ClaimTypes.MobilePhone, identityId),
            new Claim(ClaimTypes.Thumbprint, identityData!.ToJsonString())
        };

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey),
            SecurityAlgorithms.HmacSha256Signature);

        var jwt = new JwtSecurityToken(
            issuer: bearerTokenConfig.Issuer,
            audience: bearerTokenConfig.Audience,
            expires: expires??now.AddHours(Convert.ToInt32(24)),
            signingCredentials: signingCredentials,
            claims: claims
        );
        await Task.Delay(0);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
    
    /// <summary>
    /// Get the claims data from the principal
    /// </summary>
    /// <param name="principal"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetClaimsAuthData<T>(this ClaimsPrincipal principal)
    {
        var claimsIdentity = principal.Identities.FirstOrDefault();
        var claim = claimsIdentity?.FindFirst(ClaimTypes.Thumbprint);
        var auth = claimsIdentity?.FindFirst(ClaimTypes.Authentication);

        if (claim == null)
            return default!;
        var user = claim.Value.FromJsonString<T>();
        return user!;
    }

}