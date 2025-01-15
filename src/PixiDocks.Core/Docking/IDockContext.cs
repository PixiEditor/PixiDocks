namespace PixiDocks.Core.Docking;

public interface IDockContext
{
    public IDockableTarget? FocusedTarget { get; set; }
    public IReadOnlyCollection<IDockableHostRegion> AllRegions { get; }
    public IReadOnlyCollection<IDockableTarget> AllTargets { get; }
    public void AddDockableTarget(IDockableTarget target);
    public void RemoveDockableTarget(IDockableTarget target);
    public IHostWindow Float(IDockable? dockable, double x, double y);
    public void Dock(IDockable? dockable, IDockableTarget toHost);
    public IDockable? CreateDockable(IDockableContent content);
    public Task<bool> Close(IDockable? dockable);
    public void RemoveRegion(IDockableHostRegion sender);
    public void AddRegion(IDockableHostRegion sender);
    public string Serialize();
    public event Action<IDockableTarget?, bool> FocusedHostChanged;
    public bool IsFloating(IDockableHost dockableHost);
}
