using System.Collections;
using System.Text.Json.Serialization;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

[JsonConverter(typeof(DockableConverter))]
public interface IDockable : IDockableLayoutElement
{
    string Title { get;  }
    bool CanFloat { get; }
    bool CanClose { get; }
    object? Icon { get; set; }
    public IDockableHost? Host { get; set; }
    public bool CanSplit => Host?.Dockables.Count > 1;

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}