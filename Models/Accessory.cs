namespace TAM.Models;

public class Accessory
{
    public string AccessoryId { get; set; } = Guid.NewGuid().ToString();
    public string AccessoryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = "pcs";
    public decimal CurrentStock { get; set; } = 0;
    public decimal MinimumStock { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<EditHistoryEntry> EditHistory { get; set; } = new();

    [Newtonsoft.Json.JsonIgnore]
    public bool IsLowStock => MinimumStock > 0 && CurrentStock <= MinimumStock;
}
