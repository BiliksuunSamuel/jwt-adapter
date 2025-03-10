using Newtonsoft.Json;
using Scrypt;

namespace JwtAdapter.Extensions;

public static class StringExtensions
{
    
    /// <summary>
    /// Convert Object to Json String
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="indented"></param>
    /// <returns></returns>
    public static string ToJsonString(this object obj, bool indented = false)
    {
        return JsonConvert.SerializeObject(obj, indented ? Formatting.Indented : Formatting.None);
    }
    
    /// <summary>
    /// Convert JsonString to Json Object
    /// </summary>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? FromJsonString<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
    
    
    /// <summary>
    /// Method for encrypting password
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string HashPassword(this string password)
    {
        var encoder = new ScryptEncoder();
        return encoder.Encode(password);
    }
    
    
    /// <summary>
    /// Method for comparing hashed password to raw password
    /// </summary>
    /// <param name="password"></param>
    /// <param name="hashedPassword"></param>
    /// <returns></returns>
    public static bool ComparePassword(this string password, string hashedPassword)
    {
        var encoder = new ScryptEncoder();
        return encoder.Compare(password, hashedPassword);
    }
}