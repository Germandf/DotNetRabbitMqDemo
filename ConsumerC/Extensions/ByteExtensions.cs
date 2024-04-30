using System.Text;
using System.Text.Json;

namespace ConsumerC.Extensions;

public static class ByteExtensions
{
    public static T Deserialize<T>(this ReadOnlyMemory<byte> body) where T : class
    {
        var json = Encoding.UTF8.GetString(body.ToArray());
        var model = JsonSerializer.Deserialize<T>(json);

        if (model is null)
            throw new ArgumentNullException(nameof(body));

        return model;
    }
}
