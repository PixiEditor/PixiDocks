using System.Collections;
using System.Text.Json.Serialization;
using System.Windows.Input;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

[JsonConverter(typeof(DockableConverter))]
public interface IDockable : IDockableLayoutElement
{
    string Title { get;  }
    bool CanFloat { get; }
    bool CanClose { get; }
    public IDockableHost? Host { get; set; }
    public bool CanSplit => Host?.Dockables.Count > 1;
    public TabCustomizationSettings TabCustomizationSettings { get; }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}