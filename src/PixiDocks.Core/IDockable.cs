namespace PixiDocks.Core;

public interface IDockable
{
    public IDockableHost? Host { get; set; }
    public void Float();
    public void DockBack();
}