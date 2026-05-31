namespace TAM.Models;

public enum OutwardOrderStatus { Active, PartiallyReturned, FullyReturned }

public class OutwardOrderItem
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string AccessoryId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReturnedQuantity { get; set; } = 0;
}

public class OutwardOrder
{
    public string OutwardId { get; set; } = Guid.NewGuid().ToString();
    public string OutwardNumber { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public OutwardOrderStatus Status { get; set; } = OutwardOrderStatus.Active;
    public List<OutwardOrderItem> Items { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public DateTime OutwardDate { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<EditHistoryEntry> EditHistory { get; set; } = new();
}
