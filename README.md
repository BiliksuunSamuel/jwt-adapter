# JwtAdapter
## Overview
JwtAdapter is a lightweight and flexible SDK designed to simplify enabling and managing JWT-based authentication in .NET APIs. It provides an easy-to-use extension method for configuring JWT authentication, including custom validation logic and dynamic claim injection.

## Features
	•	Simple Configuration: Quickly enable JWT authentication using a single extension method.
	•	Custom Validation: Inject custom validation logic to tailor authentication to your needs.
	•	Dynamic Claims: Add dynamic claims based on validation results.
	•	Lifetime Validation: Optionally validate token expiration automatically.

## Installation
To install JwtAdapter, simply add the NuGet package to your project using the following command:
```bash
dotnet add package JwtAdapter
```

## Getting Started
1. Define BearerTokenConfig

Define the JWT configuration, such as the issuer, audience, and signing key

```csharp
services.EnableAuthentication<UserIdentity>(
    config =>
    {
        config.Issuer = "YourIssuer";
        config.Audience = "YourAudience";
        config.SigningKey = "YourSigningKey";
    },
    async id =>
    {
        // Example validation logic
        if (id == "valid-user-id")
        {
            return new IdentityValidationResults<UserIdentity>
            {
                IsSuccessful = true,
                Message = "Validation succeeded",
                IdentityData = new UserIdentity { Id = id, Name = "John Doe" }
            };
        }

        return new IdentityValidationResults<UserIdentity>
        {
            IsSuccessful = false,
            Message = "Invalid user"
        };
    });
```

2. Define Custom Validation Logic

The validationFunction takes a user ID and returns an IdentityValidationResults<T> object containing:
 - 	IsSuccessful: A boolean indicating validation success.
 - 	Message: A message explaining the validation result.
 -	IdentityData: Optional data to attach as claims.

Example IdentityValidationResults class:
```csharp
public class IdentityValidationResults<T>
{
    public bool IsSuccessful { get; set; }
    public string? Message { get; set; }
    public T? IdentityData { get; set; }
}
```

3. Add Claims Dynamically

Claims are added to the authenticated user’s identity dynamically during validation. The following example adds a serialized IdentityData and the token itself as claims:
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Thumbprint, results.IdentityData?.ToJsonString() ?? string.Empty),
    new Claim(ClaimTypes.Authentication, bearerAuth)
};
```
## Advanced Configuration
### Lifetime Validation
By default, JwtAdapter does not validate token expiration. To enable lifetime validation, set the ValidateLifetime property to true in the BearerTokenConfig object.
```csharp
services.EnableAuthentication<UserIdentity>(
    config =>
    {
        config.Issuer = "YourIssuer";
        config.Audience = "YourAudience";
        config.SigningKey = "YourSigningKey";
    },
    validationFunction,
    validateLifeTime: false
);
```
4. Hash and Compare Passwords
```csharp

    //hash password
  var rawPassword="password";
  var hashedPassword=rawPassword.HashPassword();
  
  //compare password
  var isMatch=rawPassword.ComparePassword(hashedPassword);
      
```

## Custom Error Messages
### Provide detailed error messages when validation fails:
```csharp
return new IdentityValidationResults<UserIdentity>
{
    IsSuccessful = false,
    Message = "User is not authorized to access this resource."
};
```

## Example Usage
### Startup.cs or Program.cs Configuration
```csharp 

//Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.EnableAuthentication<UserIdentity>(
        config =>
        {
            config.Issuer = "MyIssuer";
            config.Audience = "MyAudience";
            config.SigningKey = "MySecretKey";
        },
        async (id,claims) =>
        {
            // Perform custom validation
            return await ValidateUserAsync(id);
        }
    );
}

//Program.cs
 
    var builder = WebApplication.CreateBuilder(args);
    {
        var services = builder.Services;
        var config = builder.Configuration;
    
        services.EnableAuthentication<UserIdentity>(
            config =>
            {
                config.Issuer = "MyIssuer";
                config.Audience = "MyAudience";
                config.SigningKey = "MySecretKey";
            },
            async id =>
            {
                // Perform custom validation
                return await ValidateUserAsync(id);
            }
        );
    }
```

### Enable Authentication and Authorization Middleware
```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization(); // the UseAuthorization Middleware must come between the UseRouring and UseEndpoints Middleware
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

```

### Add BearTokenConfig to appsettings
```json
{
  "BearerTokenConfig": {
    "Issuer": "https://easecoreapi:com",
    "Audience": "https://easycoreapi:com",
    "SigningKey": "easycoreapi889945"
  }
}
```


### Controller Usage
```csharp

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SecureController : ControllerBase
{
    [HttpGet("protected")]
    public IActionResult GetProtectedData()
    {
        return Ok("This is protected data.");
    }
}
```

## Requirements
- .NET 6.0 or .Net 8.0
- Compatible with ASP.NET Core applications.

## Contributing
1. Fork the repository.
2. Create a new branch.
3. Make your changes.
4. Commit your changes.
5. Push your changes to your fork.
6. Submit a pull request.


## License
This project is licensed under the MIT License.


## Support

For any questions or issues,
please open an issue on GitHub or
contact us at <a href="mailto:developer.biliksuun@gmail.com">
developer.biliksuun@gmail.com</a>.

## Authors
- Samuel Biliksuun