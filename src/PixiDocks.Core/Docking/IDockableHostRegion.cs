using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

public interface IDockableHostRegion
{
    public string Id { get; }
    public IDockContext Context { get; set; }
    public IReadOnlyCollection<IDockableHost> AllHosts { get; }
    public bool CanDock();
    public IDockable ValidDockable { get; }
    public IDockableTree Root { get; set; }
}