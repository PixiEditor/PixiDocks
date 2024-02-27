using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using PixiDocks.Core;

namespace PixiDocks.Avalonia.Controls;

public class HostWindow : Window, IHostWindow
{
    protected override Type StyleKeyOverride => typeof(HostWindow);
}