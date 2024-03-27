using Avalonia.Controls;
using HexEditor.ViewModels;

namespace HexEditor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ViewModelMain(GetTopLevel(this).StorageProvider);
    }
}