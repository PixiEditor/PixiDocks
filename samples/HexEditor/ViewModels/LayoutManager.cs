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
    public LayoutTree DocumentsLayout { get; private set; }
    public LayoutTree InspectorLayout { get; private set; }

    public LayoutManager()
    {
        DocumentsLayout = new LayoutTree()
        {
            Root = new DockableTree()
            {
                First = new DockableArea()
                {
                    Id = "DocumentArea",
                    FallbackContent = new TextBlock(){Text = "Open a file to view it's hex",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center}
                },
            }
        };

        InspectorLayout = new LayoutTree()
        {
            Root = new DockableTree()
            {
                First = new DockableArea()
                {
                    Id = "InspectorArea",
                },
            }
        };

        DocumentsLayout.SetContext(DockContext);
        InspectorLayout.SetContext(DockContext);
    }
}