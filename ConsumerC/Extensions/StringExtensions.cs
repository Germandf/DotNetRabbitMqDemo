using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace ConsumerC.Extensions;

public static class StringExtensions
{
    public static bool TryDeserialize<T>(this string jsonString, [NotNullWhen(true)] out T? model) where T : class
    {
        try
        {
            model = JsonSerializer.Deserialize<T>(jsonString);
            return model != null;
        }
        catch (JsonException)
        {
            model = null;
            return false;
        }
    }
}
