namespace JwtAdapter.Options;


public class BearerTokenConfig
{
    public string? Audience { get; set; } 
    public string? Issuer { get; set; } 
    public string? SigningKey { get; set; } 
}