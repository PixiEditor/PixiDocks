using System.Text.Json.Serialization;
using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

[JsonConverter(typeof(DockableHostConverter))]
public interface IDockableHost : IDockableLayoutElement, IDockableTarget
{
    /// <summary>
    ///     If set and there are no dockables, host won't collapse and will display this content instead.
    /// </summary>
    public object? FallbackContent { get; set; }
    public void Float(IDockable? dockable);
    public void CloseHost();
    public void Close(IDockable? dockable);
    public void CloseAll();
    public void CloseAllExcept(IDockable dockable);
    public void SplitDown(IDockable? dockable);
    public void SplitLeft(IDockable? dockable);
    public void SplitRight(IDockable? dockable);
    public void SplitUp(IDockable? dockable);
    public event Action<bool> FocusedChanged;
}