using LiveSheet.Parts.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiveSheet;

public class LiveSheetSerializeConverter : JsonConverter
{
    public override bool CanRead => true; // Enable deserialization

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(LiveSheetDiagram);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var obj = new JObject();
        var type = value.GetType();
        foreach (var prop in type.GetProperties())
            if (prop.IsDefined(typeof(LiveSerialize), false))
            {
                var propValue = prop.GetValue(value);
                obj.Add(prop.Name, propValue != null ? JToken.FromObject(propValue, serializer) : JValue.CreateNull());
            }

        obj.WriteTo(writer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var instance = existingValue ?? Activator.CreateInstance(objectType); // Ensure existingValue or create new

        foreach (var prop in objectType.GetProperties())
            if (prop.IsDefined(typeof(LiveSerialize), false) && jsonObject.TryGetValue(prop.Name, out var token))
            {
                var value = token.ToObject(prop.PropertyType, serializer);
                prop.SetValue(instance, value);
            }

        return instance;
    }
}