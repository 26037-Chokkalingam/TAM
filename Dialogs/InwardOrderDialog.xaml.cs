using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using TAM.Models;
using TAM.Services;

namespace TAM.Dialogs;

public class InwardItemRow : System.ComponentModel.INotifyPropertyChanged
{
    private string _accessoryId = string.Empty;
    public string AccessoryId { get => _accessoryId; set { _accessoryId = value; OnPC(nameof(AccessoryId)); OnPC(nameof(AccessoryName)); } }
    public string? POItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal RequestedQty { get; set; }
    public string AccessoryName => DataService.Instance.GetAccessoryName(AccessoryId);
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    void OnPC(string n) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(n));
}

public partial class InwardOrderDialog : Window
{
    private PurchaseOrder? _po;
    private InwardOrder? _existing;
    public ObservableCollection<InwardItemRow> ItemRows { get; } = new();
    public IReadOnlyList<Accessory> Accessories { get; } = DataService.Instance.GetAccessories();

    // Direct / Edit mode
    public InwardOrderDialog(InwardOrder? inward = null)
    {
        InitializeComponent();
        DataContext = this;
        _existing = inward;
        VendorCombo.ItemsSource = DataService.Instance.GetVendors().Where(v => v.IsActive).ToList();
        PartialPanel.Visibility = Visibility.Collapsed;
        AddItemBtn.Visibility = Visibility.Visible;

        if (inward != null)
        {
            TitleText.Text = "Edit Inward Order";
            SubTitleText.Text = $"Editing {inward.InwardNumber}";
            VendorCombo.SelectedValue = inward.VendorId;
            BillNoBox.Text = inward.BillNo;
            InwardDatePicker.SelectedDate = inward.InwardDate;
            NotesBox.Text = inward.Notes;
            foreach (var item in inward.Items)
                ItemRows.Add(new InwardItemRow { AccessoryId = item.AccessoryId, Quantity = item.Quantity });
        }
        else
        {
            InwardDatePicker.SelectedDate = DateTime.Today;
        }
        ItemsGrid.ItemsSource = ItemRows;
    }

    // From PO mode
    public InwardOrderDialog(PurchaseOrder po)
    {
        InitializeComponent();
        DataContext = this;
        _po = po;
        VendorCombo.ItemsSource = DataService.Instance.GetVendors().Where(v => v.IsActive).ToList();
        VendorCombo.SelectedValue = po.VendorId;
        VendorCombo.IsEnabled = false;
        AddItemBtn.Visibility = Visibility.Collapsed;
        TitleText.Text = "Convert PO to Inward Order";
        PORefPanel.Visibility = Visibility.Visible;
        PORefText.Text = $"PO: {po.PONumber} | Vendor: {DataService.Instance.GetVendorName(po.VendorId)}";
        InwardDatePicker.SelectedDate = DateTime.Today;

        foreach (var item in po.Items.Where(i => i.ReceivedQuantity < i.RequestedQuantity))
        {
            ItemRows.Add(new InwardItemRow
            {
                AccessoryId = item.AccessoryId,
                POItemId = item.ItemId,
                RequestedQty = item.RequestedQuantity - item.ReceivedQuantity,
                Quantity = item.RequestedQuantity - item.ReceivedQuantity
            });
        }
        ItemsGrid.ItemsSource = ItemRows;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        try { ItemsGrid.CancelEdit(DataGridEditingUnit.Row); } catch { }
        base.OnClosing(e);
    }

    private void VendorFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (sender is not ComboBox cb) return;
        var all = DataService.Instance.GetVendors().Where(v => v.IsActive).ToList();
        if (cb.SelectedItem is Vendor sel && sel.Name == cb.Text) { cb.ItemsSource = all; return; }
        cb.ItemsSource = string.IsNullOrWhiteSpace(cb.Text)
            ? all
            : all.Where(v => v.Name.Contains(cb.Text, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!cb.IsDropDownOpen && !string.IsNullOrEmpty(cb.Text)) cb.IsDropDownOpen = true;
    }

    private void AccessoryFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (sender is not ComboBox cb) return;
        var all = DataService.Instance.GetAccessories().Where(a => a.IsActive).ToList();
        if (cb.SelectedItem is Accessory sel && sel.Name == cb.Text) { cb.ItemsSource = all; return; }
        cb.ItemsSource = string.IsNullOrWhiteSpace(cb.Text)
            ? all
            : all.Where(a => a.Name.Contains(cb.Text, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!cb.IsDropDownOpen && !string.IsNullOrEmpty(cb.Text)) cb.IsDropDownOpen = true;
    }

    private void AccessoryCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb && cb.SelectedValue is string id &&
            ItemsGrid.SelectedItem is InwardItemRow row)
            row.AccessoryId = id;
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
        => ItemRows.Add(new InwardItemRow { Quantity = 1 });

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is InwardItemRow row && _po == null)
            ItemRows.Remove(row);
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (VendorCombo.SelectedValue is not string vendorId || string.IsNullOrEmpty(vendorId))
        {
            MessageBox.Show("Please select a vendor.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(BillNoBox.Text))
        {
            MessageBox.Show("Bill Number is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!ItemRows.Any(r => r.Quantity > 0))
        {
            MessageBox.Show("Please add at least one item with quantity > 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_po != null)
        {
            // Convert PO to inward
            var received = ItemRows.ToDictionary(r => r.POItemId!, r => r.Quantity);
            bool partial = PartialRadio.IsChecked == true;
            var result = DataService.Instance.ConvertPOToInward(_po, received, BillNoBox.Text.Trim(), partial);
            if (result == null)
            {
                MessageBox.Show("No items to inward.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        else if (_existing != null)
        {
            var updatedInward = JsonConvert.DeserializeObject<InwardOrder>(
                JsonConvert.SerializeObject(_existing))!;
            updatedInward.VendorId = vendorId;
            updatedInward.BillNo = BillNoBox.Text.Trim();
            updatedInward.InwardDate = InwardDatePicker.SelectedDate ?? DateTime.Today;
            updatedInward.Notes = NotesBox.Text.Trim();
            updatedInward.Items = ItemRows.Where(r => r.Quantity > 0).Select(r => new InwardOrderItem
            {
                AccessoryId = r.AccessoryId,
                Quantity = r.Quantity
            }).ToList();
            DataService.Instance.UpdateInwardOrder(updatedInward, "Edited inward order");
        }
        else
        {
            var inward = new InwardOrder
            {
                VendorId = vendorId,
                BillNo = BillNoBox.Text.Trim(),
                InwardDate = InwardDatePicker.SelectedDate ?? DateTime.Today,
                Notes = NotesBox.Text.Trim(),
                Items = ItemRows.Where(r => r.Quantity > 0).Select(r => new InwardOrderItem
                {
                    AccessoryId = r.AccessoryId,
                    Quantity = r.Quantity
                }).ToList()
            };
            DataService.Instance.AddInwardOrder(inward);
        }

        DialogResult = true;
    }
}
