using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;

namespace SimpleSample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        LayoutTree tree = new LayoutTree();
        RootRegion.DockableArea.SplitRight(RootRegion.DockableArea.Dockables[0]);
        tree.Root = RootRegion.Output;
        tree.Traverse((IDockableLayoutElement element) =>
        {

        });
    }
}