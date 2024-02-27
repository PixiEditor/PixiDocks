namespace PixiDocks.Core;

public interface IDockableHost
{
    public IReadOnlyCollection<IDockable> Dockables { get; set; }
    public IDockable ActiveDockable { get; set; }

    public void AddDockable(IDockable dockable);
    public void RemoveDockable(IDockable dockable);
    public void SetActiveDockable(IDockable dockable);
}