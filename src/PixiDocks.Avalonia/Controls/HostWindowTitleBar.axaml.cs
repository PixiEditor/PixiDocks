using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;

namespace PixiDocks.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="HostWindowTitleBar"/> xaml.
/// </summary>
public class HostWindowTitleBar : TitleBar
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(HostWindowTitleBar);
}
