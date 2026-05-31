using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TAM.Models;
using TAM.Services;

namespace TAM.Dialogs;

public class POItemRow : System.ComponentModel.INotifyPropertyChanged
{
    private string _accessoryId = string.Empty;
    public string AccessoryId { get => _accessoryId; set { _accessoryId = value; OnPC(nameof(AccessoryId)); OnPC(nameof(AccessoryName)); OnPC(nameof(AccessoryUnit)); } }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string AccessoryName => DataService.Instance.GetAccessoryName(AccessoryId);
    public string AccessoryUnit => DataService.Instance.GetAccessoryById(AccessoryId)?.Unit ?? string.Empty;
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    void OnPC(string n) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(n));
}

public partial class PurchaseOrderDialog : Window
{
    private PurchaseOrder? _existing;
    public ObservableCollection<POItemRow> ItemRows { get; } = new();
    public IReadOnlyList<Accessory> Accessories { get; } = DataService.Instance.GetAccessories();

    public PurchaseOrderDialog(PurchaseOrder? po = null)
    {
        InitializeComponent();
        DataContext = this;
        _existing = po;

        VendorCombo.ItemsSource = DataService.Instance.GetVendors().Where(v => v.IsActive).ToList();

        if (po != null)
        {
            TitleText.Text = "Edit Purchase Order";
            VendorCombo.SelectedValue = po.VendorId;
            NotesBox.Text = po.Notes;
            foreach (var item in po.Items)
                ItemRows.Add(new POItemRow { AccessoryId = item.AccessoryId, Quantity = item.RequestedQuantity, UnitPrice = item.UnitPrice });
        }
        ItemsGrid.ItemsSource = ItemRows;
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
        => ItemRows.Add(new POItemRow { Quantity = 1 });

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is POItemRow row) ItemRows.Remove(row);
    }

    private void AccessoryCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb && cb.SelectedValue is string id &&
            ItemsGrid.SelectedItem is POItemRow row)
        {
            row.AccessoryId = id;
        }
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (VendorCombo.SelectedValue is not string vendorId || string.IsNullOrEmpty(vendorId))
        {
            MessageBox.Show("Please select a vendor.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!ItemRows.Any())
        {
            MessageBox.Show("Please add at least one item.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        foreach (var r in ItemRows)
        {
            if (string.IsNullOrEmpty(r.AccessoryId))
            {
                MessageBox.Show("Please select an accessory for all items.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (r.Quantity <= 0)
            {
                MessageBox.Show("Quantity must be greater than 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        if (_existing == null)
        {
            var po = new PurchaseOrder
            {
                VendorId = vendorId,
                Notes = NotesBox.Text.Trim(),
                Items = ItemRows.Select(r => new PurchaseOrderItem
                {
                    AccessoryId = r.AccessoryId,
                    RequestedQuantity = r.Quantity,
                    UnitPrice = r.UnitPrice
                }).ToList()
            };
            DataService.Instance.AddPurchaseOrder(po);
        }
        else
        {
            _existing.VendorId = vendorId;
            _existing.Notes = NotesBox.Text.Trim();
            _existing.Items = ItemRows.Select(r => new PurchaseOrderItem
            {
                AccessoryId = r.AccessoryId,
                RequestedQuantity = r.Quantity,
                UnitPrice = r.UnitPrice
            }).ToList();
            DataService.Instance.UpdatePurchaseOrder(_existing, "Edited purchase order");
        }

        DialogResult = true;
    }
}
