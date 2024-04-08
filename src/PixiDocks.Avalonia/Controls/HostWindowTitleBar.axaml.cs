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
    internal Control? BackgroundControl { get; private set; }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(HostWindowTitleBar);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        BackgroundControl = e.NameScope.Find<Control>("PART_Background");
    }

    public bool IsOverTab()
    {
        return BackgroundControl?.IsPointerOver ?? false;
    }
}
