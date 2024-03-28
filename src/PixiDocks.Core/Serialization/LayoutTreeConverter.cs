using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class LayoutTreeConverter : CustomConverter<LayoutTree>
{
    public override void Write(Utf8JsonWriter writer, LayoutTree value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteStartObject("Root");
        DockableTreeConverter converter = new DockableTreeConverter();
        converter.Write(writer, value.Root, options);

        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    public override LayoutTree Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        StartReadingScope(ref reader);
        LayoutTree tree = new LayoutTree();
        DockableTreeConverter converter = new DockableTreeConverter();
        if (TryReadToNextProperty(ref reader, out string propName) && propName == "Root")
        {
            StartReadingScope(ref reader);
            StartReadingProperty(ref reader);
            tree.Root = converter.Read(ref reader, LayoutTree.TypeMap[typeof(IDockableTree)], options);
        }

        EndReadingScope(ref reader);
        while(reader.Read()){}
        return tree;
    }
}