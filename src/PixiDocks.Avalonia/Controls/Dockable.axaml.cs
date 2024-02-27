using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using PixiDocks.Core;

namespace PixiDocks.Avalonia.Controls;

[TemplatePart("PART_FloatButton", typeof(Button))]
public class Dockable : ContentControl, IDockable
{
    public IDockableHost? Host { get; set; }

    private HostWindow? _hostWindow;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var tb = e.NameScope.Find<Button>("PART_FloatButton");
        tb.Click += (sender, args) =>
        {
            if (_hostWindow == null)
                Float();
            else
                DockBack();
        };
    }

    public void Float()
    {
        var content = Content;
        Content = null;

        _hostWindow = new HostWindow
        {
            Content = content,
            Width = this.Width,
            Height = this.Height,
            Position = this.PointToScreen(new Point(0, 0))
        };

        _hostWindow.Show();
    }

    public void DockBack()
    {
        object? content = _hostWindow?.Content;
        if (_hostWindow != null)
        {
            _hostWindow.Content = null;
            _hostWindow.Close();
            _hostWindow = null;
        }

        Content = content;
    }
}