using System.Text.Json;
using System.Text;

namespace Producer.Extensions;

public static class StringExtensions
{
    public static byte[] GetJsonBytes<T>(this T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        return body;
    }
}
