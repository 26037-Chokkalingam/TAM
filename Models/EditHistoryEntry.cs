namespace TAM.Models;

public class EditHistoryEntry
{
    public string EntryId { get; set; } = Guid.NewGuid().ToString();
    public DateTime ChangedAt { get; set; } = DateTime.Now;
    public string ChangeDescription { get; set; } = string.Empty;
    public string SnapshotJson { get; set; } = string.Empty;
}
