using Newtonsoft.Json;

namespace JwtAdapter.Extensions;

public static class StringExtensions
{
    public static string ToJsonString(this object obj, bool indented = false)
    {
        return JsonConvert.SerializeObject(obj, indented ? Formatting.Indented : Formatting.None);
    }
    
    public static T? FromJsonString<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}