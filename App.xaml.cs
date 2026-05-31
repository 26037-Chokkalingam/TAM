using System.Windows;

namespace TAM;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        TAM.Services.AuditService.Instance.Log("APP", "System", "Application started");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        TAM.Services.AuditService.Instance.Log("APP", "System", "Application closed");
        base.OnExit(e);
    }
}
