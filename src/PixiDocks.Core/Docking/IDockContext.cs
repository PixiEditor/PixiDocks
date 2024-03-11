namespace PixiDocks.Core.Docking;

public interface IDockContext
{
    public IReadOnlyCollection<IDockableHost> AllHosts { get; }
    public void AddHost(IDockableHost host);
    public void RemoveHost(IDockableHost host);
    public IHostWindow Float(IDockable dockable, double x, double y);
    public void Dock(IDockable dockable, IDockableHost toHost);
    public void Close(IDockable dockable);
}