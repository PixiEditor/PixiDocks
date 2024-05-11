using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class DockableHostConverter : CustomConverter<IDockableHost>
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
        StartReadingScope(ref reader);

        IDockableHost host = Activator.CreateInstance(typeToConvert) as IDockableHost;

        while (TryReadToNextProperty(ref reader, out string propName))
        {
            bool found;
            switch (propName)
            {
                case nameof(IDockableHost.Dockables):
                    found = true;
                    StartReadingScope(ref reader);
                    break;
                default:
                    found = false;
                    break;
            }

            if(found) break;
        }

        DockableConverter converter = new DockableConverter();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            IDockable? dockable = converter.Read(ref reader, LayoutTree.TypeMap[typeof(IDockable)], options);
            host.AddDockable(dockable);
        }

        EndReadingScope(ref reader);
        EndReadingScope(ref reader);
        return host;
    }
}