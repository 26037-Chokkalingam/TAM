using System.Windows;
using System.Windows.Threading;
using TAM.Services;

namespace TAM;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        AuditService.Instance.Log("APP", "System", "Application started");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AuditService.Instance.Log("APP", "System", "Application closed");
        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleError(e.Exception);
        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            HandleError(ex);
    }

    private static void HandleError(Exception ex)
    {
        try { AuditService.Instance.Log("ERROR", "System", $"Unhandled: {ex.GetType().Name} – {ex.Message}"); }
        catch { /* swallow audit failure */ }

        MessageBox.Show(
            $"An unexpected error occurred and has been logged.\n\n{ex.Message}",
            "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
