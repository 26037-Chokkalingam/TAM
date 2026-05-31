using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TAM.Models;
using TAM.Services;

namespace TAM.Dialogs;

public class OutwardItemRow : System.ComponentModel.INotifyPropertyChanged
{
    private string _accessoryId = string.Empty;
    public string AccessoryId
    {
        get => _accessoryId;
        set
        {
            _accessoryId = value;
            OnPC(nameof(AccessoryId)); OnPC(nameof(AccessoryName));
            OnPC(nameof(AvailableStock)); OnPC(nameof(Unit));
        }
    }
    public decimal Quantity { get; set; }
    public string AccessoryName => DataService.Instance.GetAccessoryName(AccessoryId);
    public decimal AvailableStock => DataService.Instance.GetAccessoryById(AccessoryId)?.CurrentStock ?? 0;
    public string Unit => DataService.Instance.GetAccessoryById(AccessoryId)?.Unit ?? string.Empty;
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    void OnPC(string n) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(n));
}

public partial class OutwardOrderDialog : Window
{
    private OutwardOrder? _existing;
    public ObservableCollection<OutwardItemRow> ItemRows { get; } = new();
    public IReadOnlyList<Accessory> Accessories { get; } = DataService.Instance.GetAccessories();

    public OutwardOrderDialog(OutwardOrder? outward = null)
    {
        InitializeComponent();
        DataContext = this;
        _existing = outward;
        OutwardDatePicker.SelectedDate = DateTime.Today;

        if (outward != null)
        {
            TitleText.Text = "Edit Outward Order";
            RecipientBox.Text = outward.Recipient;
            PurposeBox.Text = outward.Purpose;
            OutwardDatePicker.SelectedDate = outward.OutwardDate;
            NotesBox.Text = outward.Notes;
            foreach (var item in outward.Items)
                ItemRows.Add(new OutwardItemRow { AccessoryId = item.AccessoryId, Quantity = item.Quantity });
        }
        ItemsGrid.ItemsSource = ItemRows;
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
        => ItemRows.Add(new OutwardItemRow { Quantity = 1 });

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is OutwardItemRow row) ItemRows.Remove(row);
    }

    private void AccessoryCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb && cb.SelectedValue is string id &&
            ItemsGrid.SelectedItem is OutwardItemRow row)
        {
            row.AccessoryId = id;
        }
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        ErrorPanel.Visibility = Visibility.Collapsed;
        if (string.IsNullOrWhiteSpace(RecipientBox.Text))
        {
            MessageBox.Show("Recipient is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!ItemRows.Any())
        {
            MessageBox.Show("Please add at least one item.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_existing == null)
        {
            var outward = new OutwardOrder
            {
                Recipient = RecipientBox.Text.Trim(),
                Purpose = PurposeBox.Text.Trim(),
                OutwardDate = OutwardDatePicker.SelectedDate ?? DateTime.Today,
                Notes = NotesBox.Text.Trim(),
                Items = ItemRows.Select(r => new OutwardOrderItem
                {
                    AccessoryId = r.AccessoryId, Quantity = r.Quantity
                }).ToList()
            };
            if (!DataService.Instance.AddOutwardOrder(outward, out var error))
            {
                ErrorPanel.Visibility = Visibility.Visible;
                ErrorText.Text = error;
                return;
            }
        }
        else
        {
            _existing.Recipient = RecipientBox.Text.Trim();
            _existing.Purpose = PurposeBox.Text.Trim();
            _existing.OutwardDate = OutwardDatePicker.SelectedDate ?? DateTime.Today;
            _existing.Notes = NotesBox.Text.Trim();
            var newItems = ItemRows.Select(r => new OutwardOrderItem
            {
                AccessoryId = r.AccessoryId, Quantity = r.Quantity
            }).ToList();
            // preserve returned quantities
            foreach (var ni in newItems)
            {
                var old = _existing.Items.FirstOrDefault(i => i.AccessoryId == ni.AccessoryId);
                if (old != null) ni.ReturnedQuantity = old.ReturnedQuantity;
            }
            _existing.Items = newItems;
            if (!DataService.Instance.UpdateOutwardOrder(_existing, out var error))
            {
                ErrorPanel.Visibility = Visibility.Visible;
                ErrorText.Text = error;
                return;
            }
        }

        DialogResult = true;
    }
}
