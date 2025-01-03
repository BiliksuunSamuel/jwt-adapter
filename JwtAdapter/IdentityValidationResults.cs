namespace JwtAdapter;

public class IdentityValidationResults<T>
{
    public bool IsSuccessful { get; set; }
    public string? Message { get; set; }
    public T? IdentityData { get; set; }
}