namespace PixiDocks.Core.Docking.Events;

public interface IDockableSelectionEvents
{
    void OnSelected();
    void OnDeselected();
}