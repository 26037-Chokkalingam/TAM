namespace TAM.Models;

public class DetailField
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class DetailItemRow
{
    public string Col1 { get; set; } = string.Empty;
    public string Col2 { get; set; } = string.Empty;
    public string Col3 { get; set; } = string.Empty;
    public string Col4 { get; set; } = string.Empty;
}

public class DetailInfo
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<DetailField> Fields { get; set; } = new();
    public string ItemsHeader { get; set; } = "Items";
    public List<string> ItemColumns { get; set; } = new();
    public List<DetailItemRow> Items { get; set; } = new();
    public List<EditHistoryEntry> EditHistory { get; set; } = new();
}
