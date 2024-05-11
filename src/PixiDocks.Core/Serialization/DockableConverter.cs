using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class DockableConverter : CustomConverter<IDockable>
{
    public override void Write(Utf8JsonWriter writer, IDockable? value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Id", value.Id);
        writer.WriteEndObject();
    }

    public override IDockable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        StartReadingScope(ref reader);
        string id = "";
        while (TryReadToNextProperty(ref reader, out string propName))
        {
            switch (propName)
            {
                case nameof(IDockable.Id):
                    id = ReadStringProperty(ref reader);
                    break;
            }
        }
        IDockable dockable = Activator.CreateInstance(typeToConvert) as IDockable;
        dockable.Id = id;

        EndReadingScope(ref reader);
        return dockable;
    }
}