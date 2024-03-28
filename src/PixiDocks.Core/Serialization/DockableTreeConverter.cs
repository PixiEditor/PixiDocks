using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class DockableTreeConverter : CustomConverter<IDockableTree>
{
    public override void Write(Utf8JsonWriter writer, IDockableTree value, JsonSerializerOptions options)
    {
        writer.WriteStartObject("DockableTree");
        writer.WriteString("SplitDirection", value.SplitDirection.ToString());
        if (value.SplitDirection.HasValue)
        {
            writer.WriteNumber("FirstSize", value.FirstSize);
            writer.WriteNumber("SecondSize", value.SecondSize);
        }

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
        StartReadingScope(ref reader);

        DockingDirection? direction = null;
        double? firstSize = null;
        double? secondSize = null;
        IDockableLayoutElement first = null;
        IDockableLayoutElement second = null;
        DockingElementConverter converter = new DockingElementConverter();

        while (TryReadToNextProperty(ref reader, out string propName))
        {
            switch (propName)
            {
                case nameof(IDockableTree.SplitDirection):
                    direction = null;
                    if (Enum.TryParse(typeof(DockingDirection), ReadStringProperty(ref reader), out object? result))
                    {
                        direction = (DockingDirection)result;
                    }
                    break;
                case nameof(IDockableTree.FirstSize):
                    firstSize = ReadDoubleProperty(ref reader);
                    break;
                case nameof(IDockableTree.SecondSize):
                    secondSize = ReadDoubleProperty(ref reader);
                    break;
                case nameof(IDockableTree.First):
                    first = converter.Read(ref reader, null, options);
                    break;
                case nameof(IDockableTree.Second):
                    second = converter.Read(ref reader, null, options);
                    break;
            }
        }

        IDockableTree treeElement = Activator.CreateInstance(typeToConvert) as IDockableTree;
            treeElement.First = first as ITreeElement;
            treeElement.Second = second as ITreeElement;
            treeElement.SplitDirection = direction;
            if (firstSize.HasValue)
            {
                treeElement.FirstSize = firstSize.Value;
            }

            if (secondSize.HasValue)
            {
                treeElement.SecondSize = secondSize.Value;
            }

            EndReadingScope(ref reader);
            return treeElement;
        }
}