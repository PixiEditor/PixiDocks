using System.Collections.ObjectModel;
using PixiDocks.Avalonia;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;

namespace HexEditor.ViewModels;

public class LayoutManager
{
    public DockContext DockContext { get; } = new();
    public LayoutTree Layout { get; private set; }

    public LayoutManager()
    {
        Layout = new LayoutTree()
        {
            Root = new DockableTree()
            {
                First = new DockableArea()
                {
                    Id = "DocumentArea",
                },
                SplitDirection = DockingDirection.Right,
                FirstSize = 0.75,
                Second = new DockableArea()
                {
                    Id = "InspectorArea",
                }
            }
        };

        Layout.SetContext(DockContext);
    }
}