namespace TAM.Models;

public class AddonItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class AddonData
{
    public List<AddonItem> Categories { get; set; } = new();
    public List<AddonItem> Styles { get; set; } = new();
}
