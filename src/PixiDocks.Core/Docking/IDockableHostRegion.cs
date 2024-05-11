using PixiDocks.Core.Serialization;

namespace PixiDocks.Core.Docking;

public interface IDockableHostRegion
{
    public string Id { get; }
    public IDockContext Context { get; set; }
    public IReadOnlyCollection<IDockableTarget> AllTargets { get; }
    public IDockable? ValidDockable { get; }
    public IDockableTree Root { get; set; }
}