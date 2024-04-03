namespace PixiDocks.Core.Docking;

public interface IDockContext
{
    public IDockableHost? FocusedHost { get; set; }
    public IReadOnlyCollection<IDockableHostRegion> AllRegions { get; }
    public IReadOnlyCollection<IDockableHost> AllHosts { get; }
    public void AddHost(IDockableHost host);
    public void RemoveHost(IDockableHost host);
    public IHostWindow Float(IDockable dockable, double x, double y);
    public void Dock(IDockable dockable, IDockableHost toHost);
    public IDockable CreateDockable(IDockableContent content);
    public void Close(IDockable dockable);
    public void RemoveRegion(IDockableHostRegion sender);
    public void AddRegion(IDockableHostRegion sender);
    public string Serialize();
    public event Action<IDockableHost?> FocusedHostChanged;
}