using System.Text.Json;
using System.Text.Json.Serialization;
using PixiDocks.Core.Docking;

namespace PixiDocks.Core.Serialization;

public class DockingElementConverter : CustomConverter<IDockableLayoutElement>
{
    public override IDockableLayoutElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        StartReadingScope(ref reader);

        Type finalType = null;
        while (TryReadToNextProperty(ref reader, out string propName))
        {
            if(LayoutTree.TypeResolver.TryGetValue(propName, out var value))
            {
                finalType = LayoutTree.TypeMap[value];
                break;
            }
        }

        if (finalType != null)
        {
            if (finalType.IsAssignableTo(typeof(IDockableHost)))
            {
                DockableHostConverter converter = new DockableHostConverter();
                var result = converter.Read(ref reader, finalType, options);
                EndReadingScope(ref reader);
                return result;
            }

            if (finalType.IsAssignableTo(typeof(IDockableTree)))
            {
                DockableTreeConverter converter = new DockableTreeConverter();
                var result = converter.Read(ref reader, finalType, options);
                EndReadingScope(ref reader);
                return result;
            }
        }

        EndReadingScope(ref reader);
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