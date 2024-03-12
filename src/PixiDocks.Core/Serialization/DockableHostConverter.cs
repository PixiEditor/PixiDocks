using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class DockableHostConverter : JsonConverter<IDockableHost>
{
    public override void Write(Utf8JsonWriter writer, IDockableHost value, JsonSerializerOptions options)
    {
        writer.WriteStartObject("DockableArea");
        writer.WriteStartArray("Dockables");
        foreach (var dockable in value.Dockables)
        {
            DockableConverter converter = new DockableConverter();
            converter.Write(writer, dockable, options);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public override IDockableHost? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();

        IDockableHost host = Activator.CreateInstance(typeToConvert) as IDockableHost;

        // Read the array start
        reader.Read();
        reader.Read();

        DockableConverter converter = new DockableConverter();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            IDockable dockable = converter.Read(ref reader, LayoutTree.TypeMap[typeof(IDockable)], options);
            host.AddDockable(dockable);
            reader.Read();
        }

        return host;
    }
}