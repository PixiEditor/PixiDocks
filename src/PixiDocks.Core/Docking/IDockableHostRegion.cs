using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

public interface IDockableHostRegion
{
    public IReadOnlyCollection<IDockableHost> AllHosts { get; }
    public bool CanDock();
    public IDockable ValidDockable { get; }
}