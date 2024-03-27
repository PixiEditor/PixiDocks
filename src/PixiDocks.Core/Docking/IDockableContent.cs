namespace PixiDocks.Core.Docking;

public interface IDockableContent
{
    string Id { get;  }
    string Title { get;  }
    bool CanFloat { get; }
    bool CanClose { get; }
    object? Icon { get; }
}