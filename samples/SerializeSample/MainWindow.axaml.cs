using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PixiDocks.Avalonia;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;

namespace SerializeSample;

public partial class MainWindow : Window
{
    private List<Dockable> _dockables = new List<Dockable>();
    public DockContext DockContext { get; } = new DockContext();

    public MainWindow()
    {
        InitializeComponent();
        RootRegion.Context = DockContext;
        Dockable someDockable = new Dockable();
        someDockable.Id = "SomeDockable";
        someDockable.Title = "Some Dockable";
        someDockable.Content = new TextBlock { Text = "Some Dockable" };
        _dockables.Add(someDockable);

        Dockable someDockable2 = new Dockable();
        someDockable2.Id = "SomeDockable2";
        someDockable2.Title = "Some Dockable 2";
        someDockable2.Content = new TextBlock { Text = "Some Dockable 2" };
        _dockables.Add(someDockable2);

        Dockable someDockable3 = new Dockable();
        someDockable3.Id = "SomeDockable3";
        someDockable3.Title = "Some Dockable 3";
        someDockable3.Content = new TextBlock { Text = "Some Dockable 3" };
        _dockables.Add(someDockable3);

        Dockable someDockable4 = new Dockable();
        someDockable4.Id = "SomeDockable4";
        someDockable4.Title = "Some Dockable 4";
        someDockable4.Content = new TextBlock { Text = "Some Dockable 4" };
        _dockables.Add(someDockable4);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        LayoutTree newTree = JsonSerializer.Deserialize<LayoutTree>(File.ReadAllText("serializedLayout.json"));
        newTree.SetContext(new DockContext());
        newTree.ApplyDockables(_dockables.Cast<IDockable>().ToList());

        RootRegion.Root = newTree.Root as DockableTree;
    }

    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        LayoutTree tree = new LayoutTree();
        tree.Root = RootRegion.Root;

        File.WriteAllText("runtimeSerializedLayout.json", JsonSerializer.Serialize(tree, options));
    }

    private void Load_OnClick(object? sender, RoutedEventArgs e)
    {
        LayoutTree newTree = JsonSerializer.Deserialize<LayoutTree>(File.ReadAllText("runtimeSerializedLayout.json"));
        newTree.SetContext(new DockContext());
        newTree.ApplyDockables(_dockables.Cast<IDockable>().ToList());

        RootRegion.Root = newTree.Root as DockableTree;
    }
}