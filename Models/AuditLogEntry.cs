namespace TAM.Models;

public class AuditLogEntry
{
    public string LogId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
}
