using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class DockableConverter : JsonConverter<IDockable>
{
    public override void Write(Utf8JsonWriter writer, IDockable value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Id", value.Id);
        writer.WriteEndObject();
    }

    public override IDockable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();

        string id = reader.GetString();
        IDockable dockable = Activator.CreateInstance(typeToConvert) as IDockable;
        dockable.Id = id;

        reader.Read();
        return dockable;
    }
}