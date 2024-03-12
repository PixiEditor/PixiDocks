using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class DockableTreeConverter : JsonConverter<IDockableTree>
{
    public override void Write(Utf8JsonWriter writer, IDockableTree value, JsonSerializerOptions options)
    {
        writer.WriteStartObject("DockableTree");
        writer.WriteString("SplitDirection", value.SplitDirection.ToString());
        writer.WriteStartObject("First");
        DockingElementConverter converter = new DockingElementConverter();
        converter.Write(writer, value.First, options);
        writer.WriteEndObject();
        writer.WriteStartObject("Second");
        converter.Write(writer, value.Second, options);

        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    public override IDockableTree? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();
        reader.Read();

        DockingDirection? direction = null;

        if (Enum.TryParse(typeof(DockingDirection), reader.GetString(), out object? result))
        {
            direction = (DockingDirection)result;
        }

        reader.Read();
        reader.Read();
        reader.Read();

        IDockableLayoutElement first = null;
        DockingElementConverter converter = new DockingElementConverter();

        if (reader.TokenType != JsonTokenType.EndObject)
        {
            string propertyName = reader.GetString();
            Type type = LayoutTree.TypeResolver[propertyName];

            first = converter.Read(ref reader, LayoutTree.TypeMap[type], options);
            reader.Read();
        }

        reader.Read();
        reader.Read();
        reader.Read();
        reader.Read();

        IDockableLayoutElement second = null;

        if (reader.TokenType != JsonTokenType.EndObject)
        {
            string propertyName = reader.GetString();
            Type type = LayoutTree.TypeResolver[propertyName];
            second = converter.Read(ref reader, LayoutTree.TypeMap[type], options);
        }

        IDockableTree treeElement = Activator.CreateInstance(typeToConvert) as IDockableTree;
        treeElement.First = first as ITreeElement;
        treeElement.Second = second as ITreeElement;
        treeElement.SplitDirection = direction;

        return treeElement;
    }
}