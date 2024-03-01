namespace PixiDocks.Core;

public interface IDockContext
{
    public IReadOnlyCollection<IDockableHost> AllHosts { get; }
    public void AddHost(IDockableHost host);
    public void RemoveHost(IDockableHost host);
    public IHostWindow Float(IDockable dockable);
    public void Dock(IDockable dockable, IDockableHost toHost);
}