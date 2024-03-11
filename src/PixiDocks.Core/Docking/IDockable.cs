using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

public interface IDockable : IDockableLayoutElement
{
    public string Id { get; }
    public string Title { get; }
    public bool CanFloat { get; }
    public bool CanClose { get; }
    public object? Icon { get; }
    public IDockableHost? Host { get; set; }
    public bool CanSplit => Host?.Dockables.Count > 1;
}