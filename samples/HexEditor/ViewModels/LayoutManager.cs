using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
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
                    FallbackContent = new TextBlock { Text = "Open a file to view it's hex",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center }
                },
                FirstSize = 0.75,
                SplitDirection = DockingDirection.Right,
                Second = new DockableArea()
                {
                    Id = "InspectorArea",
                },
            }
        };

        Layout.SetContext(DockContext);
    }
}