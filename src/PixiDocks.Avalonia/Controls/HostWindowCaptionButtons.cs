using Avalonia.Controls.Chrome;
using Avalonia.Threading;
using PixiDocks.Core.Docking;

namespace PixiDocks.Avalonia.Controls;

public class HostWindowCaptionButtons : CaptionButtons
{
    protected override Type StyleKeyOverride => typeof(CaptionButtons);

    protected override void OnClose()
    {
        if (VisualRoot is HostWindow hostWindow)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                foreach (var regionAllTarget in hostWindow.Region.AllTargets)
                {
                    for (int i = 0; i < regionAllTarget.Dockables.Count; i++)
                    {
                        var dockable = regionAllTarget.ActiveDockable; 
                        if (dockable == null) continue;

                        regionAllTarget.ActiveDockable = dockable;

                        bool close = await hostWindow.Region.Context.Close(dockable);
                        if (!close)
                        {
                            return;
                        }

                        i--;
                    }
                }

                if (hostWindow.Region.ValidDockable == null)
                {
                    base.OnClose();
                }
            });

            return;
        }

        base.OnClose();
    }
}