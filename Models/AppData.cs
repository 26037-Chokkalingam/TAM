namespace TAM.Models;

public class AppData
{
    public string ExportedAt { get; set; } = DateTime.Now.ToString("o");
    public string Version { get; set; } = "1.0.0";
    public List<Vendor> Vendors { get; set; } = new();
    public List<Accessory> Accessories { get; set; } = new();
    public List<PurchaseOrder> PurchaseOrders { get; set; } = new();
    public List<InwardOrder> InwardOrders { get; set; } = new();
    public List<OutwardOrder> OutwardOrders { get; set; } = new();
    public List<ReturnOrder> ReturnOrders { get; set; } = new();
}
