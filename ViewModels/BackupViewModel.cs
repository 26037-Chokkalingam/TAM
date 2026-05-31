using System.IO;
using System.Windows;
using TAM.Helpers;
using TAM.Services;

namespace TAM.ViewModels;

public class BackupViewModel : BaseViewModel, IRefreshable
{
    private string _statusMessage = string.Empty;
    private bool _isSuccess;

    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool IsSuccess { get => _isSuccess; set => SetProperty(ref _isSuccess, value); }

    public RelayCommand ExportCommand { get; }
    public RelayCommand ImportCommand { get; }
    public RelayCommand ExportInwardExcelCommand { get; }
    public RelayCommand ExportOutwardExcelCommand { get; }
    public RelayCommand ExportPOExcelCommand { get; }

    public BackupViewModel()
    {
        ExportCommand = new RelayCommand(_ => ExportBackup());
        ImportCommand = new RelayCommand(_ => ImportBackup());
        ExportInwardExcelCommand = new RelayCommand(_ => ExportReport("inward"));
        ExportOutwardExcelCommand = new RelayCommand(_ => ExportReport("outward"));
        ExportPOExcelCommand = new RelayCommand(_ => ExportReport("po"));
    }

    public void Refresh() { StatusMessage = string.Empty; }

    private void ExportBackup()
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON Backup|*.json",
            FileName = $"TAM_Backup_{DateTime.Now:yyyyMMdd_HHmm}"
        };
        if (dlg.ShowDialog() != true) return;
        if (BackupService.Instance.ExportBackup(dlg.FileName, out var error))
        {
            IsSuccess = true;
            StatusMessage = $"Backup exported: {dlg.FileName}";
        }
        else { IsSuccess = false; StatusMessage = $"Export failed: {error}"; }
    }

    private void ImportBackup()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "JSON Backup|*.json" };
        if (dlg.ShowDialog() != true) return;

        var preview = BackupService.Instance.PreviewBackup(dlg.FileName, out var error);
        if (preview == null) { MessageBox.Show($"Invalid backup: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }

        var importDlg = new TAM.Dialogs.ImportDialog(preview);
        if (importDlg.ShowDialog() != true) return;

        if (BackupService.Instance.ImportBackup(dlg.FileName, importDlg.ClearExisting, out error, out _))
        {
            IsSuccess = true;
            StatusMessage = $"Import successful from: {Path.GetFileName(dlg.FileName)}";
            MessageBox.Show("Data imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else { IsSuccess = false; StatusMessage = $"Import failed: {error}"; }
    }

    private void ExportReport(string type)
    {
        var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Excel|*.xlsx", FileName = $"TAM_{type}_{DateTime.Now:yyyyMMdd}" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            switch (type)
            {
                case "inward": ReportService.Instance.ExportInwardReport(dlg.FileName); break;
                case "outward": ReportService.Instance.ExportOutwardReport(dlg.FileName); break;
                case "po": ReportService.Instance.ExportPurchaseOrderReport(dlg.FileName); break;
            }
            IsSuccess = true; StatusMessage = $"Report exported: {dlg.FileName}";
        }
        catch (Exception ex) { IsSuccess = false; StatusMessage = $"Export failed: {ex.Message}"; }
    }
}
