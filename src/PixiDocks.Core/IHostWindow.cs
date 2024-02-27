namespace PixiDocks.Core;

public interface IHostWindow
{
    public IDockable ActiveDockable { get; }
}