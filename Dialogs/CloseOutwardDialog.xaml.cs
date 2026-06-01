using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using TAM.Models;
using TAM.Services;

namespace TAM.Dialogs;

public class CloseOrderItemRow : INotifyPropertyChanged
{
    private decimal _usedQuantity;

    public string ItemId { get; set; } = string.Empty;
    public string AccessoryId { get; set; } = string.Empty;
    public string AccessoryName => DataService.Instance.GetAccessoryName(AccessoryId);
    public decimal Quantity { get; set; }
    public decimal ReturnedQuantity { get; set; }
    public decimal UsedQuantity
    {
        get => _usedQuantity;
        set { _usedQuantity = value; OnPC(nameof(UsedQuantity)); OnPC(nameof(InHand)); }
    }
    public decimal InHand => Quantity - ReturnedQuantity - UsedQuantity;

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPC(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public partial class CloseOutwardDialog : Window
{
    private readonly OutwardOrder _outward;
    public ObservableCollection<CloseOrderItemRow> ItemRows { get; } = new();

    public CloseOutwardDialog(OutwardOrder outward)
    {
        InitializeComponent();
        DataContext = this;
        _outward = JsonConvert.DeserializeObject<OutwardOrder>(JsonConvert.SerializeObject(outward))!;

        SubtitleText.Text = $"{outward.OutwardNumber}  ·  {outward.Recipient}";

        foreach (var item in outward.Items)
        {
            ItemRows.Add(new CloseOrderItemRow
            {
                ItemId = item.ItemId,
                AccessoryId = item.AccessoryId,
                Quantity = item.Quantity,
                ReturnedQuantity = item.ReturnedQuantity,
                UsedQuantity = item.UsedQuantity
            });
        }
        ItemsGrid.ItemsSource = ItemRows;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        try { ItemsGrid.CancelEdit(DataGridEditingUnit.Row); } catch { }
        base.OnClosing(e);
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        ErrorPanel.Visibility = Visibility.Collapsed;

        // Commit any in-progress edit
        try { ItemsGrid.CommitEdit(DataGridEditingUnit.Row, true); } catch { }

        var usedQtys = new Dictionary<string, decimal>();
        foreach (var row in ItemRows)
        {
            if (row.UsedQuantity < 0)
            {
                ShowError("Used quantity cannot be negative.");
                return;
            }
            if (row.UsedQuantity + row.ReturnedQuantity > row.Quantity)
            {
                ShowError($"Used + Returned ({row.UsedQuantity + row.ReturnedQuantity}) exceeds dispatched ({row.Quantity}) for '{row.AccessoryName}'.");
                return;
            }
            usedQtys[row.ItemId] = row.UsedQuantity;
        }

        _outward.Notes = string.IsNullOrWhiteSpace(NotesBox.Text) ? _outward.Notes : NotesBox.Text.Trim();

        if (!DataService.Instance.CloseOutwardOrder(_outward, usedQtys, out var error))
        {
            ShowError(error);
            return;
        }

        DialogResult = true;
    }

    private void ShowError(string msg)
    {
        ErrorText.Text = msg;
        ErrorPanel.Visibility = Visibility.Visible;
    }
}
