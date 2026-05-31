using System.IO;
using Newtonsoft.Json;
using TAM.Models;

namespace TAM.Services;

public class BackupService
{
    public static BackupService Instance { get; } = new();

    public bool ExportBackup(string filePath, out string error)
    {
        error = string.Empty;
        try
        {
            var data = DataService.Instance.ExportData();
            data.ExportedAt = DateTime.Now.ToString("o");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            AuditService.Instance.Log("EXPORT", "Backup", $"Data exported to: {Path.GetFileName(filePath)}");
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }

    public bool ImportBackup(string filePath, bool clearExisting, out string error, out AppData? preview)
    {
        error = string.Empty;
        preview = null;
        try
        {
            var json = File.ReadAllText(filePath);
            preview = JsonConvert.DeserializeObject<AppData>(json);
            if (preview == null) { error = "Invalid backup file."; return false; }
            DataService.Instance.ImportData(preview, clearExisting);
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }

    public AppData? PreviewBackup(string filePath, out string error)
    {
        error = string.Empty;
        try
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<AppData>(json) ?? throw new InvalidOperationException("Invalid format");
        }
        catch (Exception ex) { error = ex.Message; return null; }
    }
}
