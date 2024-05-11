namespace PixiDocks.Core.Docking;

public interface IDockableTarget
{
    public string Id { get; set; }
    public IReadOnlyCollection<IDockable?> Dockables { get;}
    public IDockable? ActiveDockable { get; set; }
    public IDockContext Context { get; set; }
    public int DockingOrder { get; }
    public void Dock(IDockable? dockable);
    public void AddDockable(IDockable? dockable);
    public void RemoveDockable(IDockable? dockable);
    public bool IsPointWithin(int x, int y);
    public void OnDockableEntered(IDockableHostRegion region, int x, int y);
    public void OnDockableOver(IDockableHostRegion region, int x, int y);
    public void OnDockableExited(IDockableHostRegion region, int x, int y);
    public bool CanDock();
}