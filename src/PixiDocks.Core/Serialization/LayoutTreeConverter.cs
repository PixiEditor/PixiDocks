using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class LayoutTreeConverter : JsonConverter<LayoutTree>
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
        LayoutTree tree = new LayoutTree();
        reader.Read();
        reader.Read();
        DockableTreeConverter converter = new DockableTreeConverter();
        reader.Read();
        tree.Root = converter.Read(ref reader, LayoutTree.TypeMap[typeof(IDockableTree)], options);

        while(reader.Read()){}

        return tree;
    }
}