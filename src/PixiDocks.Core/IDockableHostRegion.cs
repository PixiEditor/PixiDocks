namespace PixiDocks.Core;

public interface IDockableHostRegion
{
    public IReadOnlyCollection<IDockableHost> AllHosts { get; }
    public bool CanDock();
    public IDockable ValidDockable { get; }
}