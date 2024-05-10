using LiveSheet.Parts.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LiveSheet.Parts.Serialization;

internal static class Serializer
{
    public static string Serialize(this LiveNode node, Formatting formatting = Formatting.None)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        var properties = node.ToDictionary();

        return JsonConvert.SerializeObject(properties, formatting, settings);
    }

    public static Dictionary<string, object?> ToDictionary(this LiveNode node)
    {
        var properties = new Dictionary<string, object?>();

        foreach (var prop in node.GetType().GetProperties()
                     .Where(prop => Attribute.IsDefined(prop, typeof(LiveSerialize))))
            try
            {
                var value = prop.GetValue(node);
                properties.Add(prop.Name, value);
            }
            catch
            {
                // ignored
            }

        return properties;
    }

    public static T Deserialize<T>(string json) where T : new()
    {
        var obj = new T();

        var properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        if (properties == null)
            return obj;

        foreach (var prop in typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(LiveSerialize))))
            if (properties.TryGetValue(prop.Name, out var value))
                try
                {
                    var typedValue = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(obj, typedValue);
                }
                catch
                {
                    // ignored
                }

        return obj;
    }
}