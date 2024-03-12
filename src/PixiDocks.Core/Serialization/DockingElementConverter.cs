using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class DockingElementConverter : JsonConverter<IDockableLayoutElement>
{
    public override IDockableLayoutElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if(typeToConvert.IsAssignableTo(typeof(IDockableHost)))
        {
            DockableHostConverter converter = new DockableHostConverter();
            return converter.Read(ref reader, typeToConvert, options);
        }
        if (typeToConvert.IsAssignableTo(typeof(IDockableTree)))
        {
            DockableTreeConverter converter = new DockableTreeConverter();
            return converter.Read(ref reader, typeToConvert, options);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, IDockableLayoutElement value, JsonSerializerOptions options)
    {
        if(value is IDockableHost host)
        {
            DockableHostConverter converter = new DockableHostConverter();
            converter.Write(writer, host, options);
        }
        else if (value is IDockableTree tree)
        {
            DockableTreeConverter converter = new DockableTreeConverter();
            converter.Write(writer, tree, options);
        }
    }
}