using System.IO;
using Newtonsoft.Json;
using TAM.Models;

namespace TAM.Services;

public class AuditService
{
    private static AuditService? _instance;
    public static AuditService Instance => _instance ??= new AuditService();

    private const int MaxEntries = 5000;
    private readonly string _logFile;
    private readonly object _lock = new();

    private AuditService()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TAM", "Logs");
        Directory.CreateDirectory(logDir);
        _logFile = Path.Combine(logDir, "audit.json");
    }

    public void Log(string action, string module, string description, string entityId = "")
    {
        lock (_lock)
        {
            var entries = LoadEntries();
            entries.Insert(0, new AuditLogEntry
            {
                Action = action,
                Module = module,
                Description = description,
                EntityId = entityId,
                Timestamp = DateTime.Now
            });
            if (entries.Count > MaxEntries)
                entries = entries.Take(MaxEntries).ToList();
            SaveEntries(entries);
        }
    }

    public List<AuditLogEntry> GetEntries(int count = 50)
        => LoadEntries().Take(count).ToList();

    public int GetTotalCount() => LoadEntries().Count;

    private List<AuditLogEntry> LoadEntries()
    {
        if (!File.Exists(_logFile)) return new();
        return JsonConvert.DeserializeObject<List<AuditLogEntry>>(File.ReadAllText(_logFile)) ?? new();
    }

    private void SaveEntries(List<AuditLogEntry> entries)
        => File.WriteAllText(_logFile, JsonConvert.SerializeObject(entries, Formatting.Indented));
}
