namespace TAM.Models;

public class InwardOrderItem
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string AccessoryId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class InwardOrder
{
    public string InwardId { get; set; } = Guid.NewGuid().ToString();
    public string InwardNumber { get; set; } = string.Empty;
    public string? POId { get; set; }
    public string BillNo { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
    public List<InwardOrderItem> Items { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public DateTime InwardDate { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<EditHistoryEntry> EditHistory { get; set; } = new();
}
