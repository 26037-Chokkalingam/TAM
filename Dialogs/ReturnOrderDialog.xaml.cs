using System.Collections.ObjectModel;
using System.Windows;
using TAM.Models;
using TAM.Services;

namespace TAM.Dialogs;

public class ReturnItemRow
{
    public string AccessoryId { get; set; } = string.Empty;
    public string? OriginalItemId { get; set; }
    public decimal OriginalQty { get; set; }
    public decimal AlreadyReturned { get; set; }
    public decimal MaxReturnable => OriginalQty - AlreadyReturned;
    public decimal ReturnQty { get; set; }
    public string AccessoryName => DataService.Instance.GetAccessoryName(AccessoryId);
}

public partial class ReturnOrderDialog : Window
{
    private OutwardOrder? _outward;
    private ReturnOrder? _existingReturn;
    public ObservableCollection<ReturnItemRow> ItemRows { get; } = new();

    // Create new return from outward
    public ReturnOrderDialog(OutwardOrder outward)
    {
        InitializeComponent();
        DataContext = this;
        _outward = outward;
        OutwardNoText.Text = outward.OutwardNumber;
        RecipientText.Text = outward.Recipient;
        ReturnDatePicker.SelectedDate = DateTime.Today;

        foreach (var item in outward.Items.Where(i => i.ReturnedQuantity < i.Quantity))
        {
            ItemRows.Add(new ReturnItemRow
            {
                AccessoryId = item.AccessoryId,
                OriginalItemId = item.ItemId,
                OriginalQty = item.Quantity,
                AlreadyReturned = item.ReturnedQuantity,
                ReturnQty = item.Quantity - item.ReturnedQuantity
            });
        }
        ItemsGrid.ItemsSource = ItemRows;
    }

    // Edit existing return
    public ReturnOrderDialog(ReturnOrder ret)
    {
        InitializeComponent();
        DataContext = this;
        _existingReturn = ret;
        TitleText.Text = "Edit Return Order";
        SubTitleText.Text = $"Editing {ret.ReturnNumber}";
        ReturnTypePanel.Visibility = Visibility.Collapsed;

        var outward = DataService.Instance.GetOutwardById(ret.OutwardId);
        OutwardNoText.Text = outward?.OutwardNumber ?? "-";
        RecipientText.Text = outward?.Recipient ?? "-";
        ReturnDatePicker.SelectedDate = ret.ReturnDate;
        NotesBox.Text = ret.Notes;
        FullReturnRadio.IsChecked = ret.IsFullReturn;

        foreach (var item in ret.Items)
        {
            var outItem = outward?.Items.FirstOrDefault(i => i.AccessoryId == item.AccessoryId);
            ItemRows.Add(new ReturnItemRow
            {
                AccessoryId = item.AccessoryId,
                OriginalQty = outItem?.Quantity ?? item.ReturnedQuantity,
                AlreadyReturned = 0,
                ReturnQty = item.ReturnedQuantity
            });
        }
        ItemsGrid.ItemsSource = ItemRows;
    }

    private void ReturnType_Changed(object sender, System.Windows.RoutedEventArgs e)
    {
        if (FullReturnRadio.IsChecked == true)
        {
            foreach (var r in ItemRows) r.ReturnQty = r.MaxReturnable;
            ItemsGrid.Items.Refresh();
        }
    }

    private void SaveBtn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (!ItemRows.Any(r => r.ReturnQty > 0))
        {
            MessageBox.Show("Please enter return quantity for at least one item.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        foreach (var r in ItemRows)
        {
            if (r.ReturnQty > r.MaxReturnable && _existingReturn == null)
            {
                MessageBox.Show($"Return quantity for '{r.AccessoryName}' exceeds maximum returnable ({r.MaxReturnable}).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        if (_outward != null)
        {
            bool fullReturn = FullReturnRadio.IsChecked == true;
            var qtys = ItemRows.Where(r => r.ReturnQty > 0)
                               .ToDictionary(r => r.OriginalItemId ?? r.AccessoryId, r => r.ReturnQty);
            // Use ItemId for lookup in DataService
            var itemIdQtys = new Dictionary<string, decimal>();
            foreach (var item in _outward.Items)
            {
                var row = ItemRows.FirstOrDefault(r => r.AccessoryId == item.AccessoryId);
                if (row != null && row.ReturnQty > 0)
                    itemIdQtys[item.ItemId] = row.ReturnQty;
            }
            var result = DataService.Instance.CreateReturn(_outward, itemIdQtys,
                NotesBox.Text.Trim(), fullReturn);
            if (result == null)
            {
                MessageBox.Show("Could not create return order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        else if (_existingReturn != null)
        {
            _existingReturn.ReturnDate = ReturnDatePicker.SelectedDate ?? DateTime.Today;
            _existingReturn.Notes = NotesBox.Text.Trim();
            _existingReturn.IsFullReturn = FullReturnRadio.IsChecked == true;
            _existingReturn.Items = ItemRows.Where(r => r.ReturnQty > 0).Select(r => new ReturnOrderItem
            {
                AccessoryId = r.AccessoryId,
                ReturnedQuantity = r.ReturnQty
            }).ToList();
            DataService.Instance.UpdateReturnOrder(_existingReturn, "Edited return order");
        }

        DialogResult = true;
    }
}
