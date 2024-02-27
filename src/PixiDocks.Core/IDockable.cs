namespace PixiDocks.Core;

public interface IDockable
{
    public string Id { get; }
    public string Title { get; }
    public IDockableHost? Host { get; set; }
}