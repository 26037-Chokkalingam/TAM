namespace TAM.Models;

public enum POStatus { Draft, PartiallyInward, Completed, Cancelled }

public class PurchaseOrderItem
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string AccessoryId { get; set; } = string.Empty;
    public decimal RequestedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PurchaseOrder
{
    public string POId { get; set; } = Guid.NewGuid().ToString();
    public string PONumber { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
    public POStatus Status { get; set; } = POStatus.Draft;
    public List<PurchaseOrderItem> Items { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<EditHistoryEntry> EditHistory { get; set; } = new();
}
