using Newtonsoft.Json;

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
}