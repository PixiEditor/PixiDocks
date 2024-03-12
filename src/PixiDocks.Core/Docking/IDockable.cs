using System.Text.Json.Serialization;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

[JsonConverter(typeof(DockableConverter))]
public interface IDockable : IDockableLayoutElement
{
    public string Id { get; set; }
    public string Title { get; set; }
    public bool CanFloat { get; set; }
    public bool CanClose { get; set; }
    public object? Icon { get; set; }
    public IDockableHost? Host { get; set; }
    public bool CanSplit => Host?.Dockables.Count > 1;
}