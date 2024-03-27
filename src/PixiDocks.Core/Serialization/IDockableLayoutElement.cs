using System.Text.Json.Serialization;

namespace PixiDocks.Core.Serialization;

[JsonConverter(typeof(DockingElementConverter))]
public interface IDockableLayoutElement : IEnumerable<IDockableLayoutElement>
{
    public string Id { get; set; }
}