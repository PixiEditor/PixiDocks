using System.Text.Json.Serialization;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

[JsonConverter(typeof(DockableHostConverter))]
public interface IDockableHost : IDockableLayoutElement
{
    public IDockContext Context { get; set; }
    public IReadOnlyCollection<IDockable> Dockables { get;}
    public IDockable ActiveDockable { get; set; }
    /// <summary>
    ///     If set and there are no dockables, host won't collapse and will display this content instead.
    /// </summary>
    public object? FallbackContent { get; set; }
    public void AddDockable(IDockable dockable);
    public void RemoveDockable(IDockable dockable);
    public bool IsDockableWithin(int x, int y);
    public void OnDockableEntered(IDockableHostRegion region, int x, int y);
    public void OnDockableOver(IDockableHostRegion region, int x, int y);
    public void OnDockableExited(IDockableHostRegion region, int x, int y);
    public void Dock(IDockable dockable);
    public void Float(IDockable dockable);
    public void Close(IDockable dockable);
    public void CloseAll();
    public void CloseAllExcept(IDockable dockable);
    public void SplitDown(IDockable dockable);
    public void SplitLeft(IDockable dockable);
    public void SplitRight(IDockable dockable);
    public void SplitUp(IDockable dockable);
    public event Action<bool> FocusedChanged;
}