namespace TAM.Models;

public class ReturnOrderItem
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string AccessoryId { get; set; } = string.Empty;
    public decimal ReturnedQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ReturnOrder
{
    public string ReturnId { get; set; } = Guid.NewGuid().ToString();
    public string ReturnNumber { get; set; } = string.Empty;
    public string OutwardId { get; set; } = string.Empty;
    public bool IsFullReturn { get; set; }
    public List<ReturnOrderItem> Items { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<EditHistoryEntry> EditHistory { get; set; } = new();
}
