using System.Windows;

namespace MaomaoDesktopPet;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Services.AppServices.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try { Services.AppServices.Save.Save(Services.AppServices.Data); }
        catch { /* ignore */ }
        base.OnExit(e);
    }
}
