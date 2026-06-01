using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
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

        // Populate recipient vendor combo
        var vendors = DataService.Instance.GetVendors().Where(v => v.IsActive).ToList();
        RecipientCombo.ItemsSource = vendors;

        // Populate style combo
        LoadStyles();

        if (outward != null)
        {
            TitleText.Text = "Edit Outward Order";
            var matchedVendor = vendors.FirstOrDefault(v => v.Name == outward.Recipient);
            if (matchedVendor != null)
                RecipientCombo.SelectedItem = matchedVendor;
            else
                RecipientCombo.Text = outward.Recipient;

            PurposeBox.Text = outward.Purpose;
            StyleCombo.Text = outward.Style;
            OutwardDatePicker.SelectedDate = outward.OutwardDate;
            NotesBox.Text = outward.Notes;
            foreach (var item in outward.Items)
                ItemRows.Add(new OutwardItemRow { AccessoryId = item.AccessoryId, Quantity = item.Quantity });
        }
        ItemsGrid.ItemsSource = ItemRows;
    }

    private void LoadStyles()
    {
        var styles = DataService.Instance.GetStyles().Select(s => s.Name).ToList();
        StyleCombo.ItemsSource = styles;
    }

    private void StyleFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (sender is not ComboBox cb) return;
        var all = DataService.Instance.GetStyles().Select(s => s.Name).ToList();
        if (cb.SelectedItem is string sel && sel == cb.Text) { cb.ItemsSource = all; return; }
        cb.ItemsSource = string.IsNullOrWhiteSpace(cb.Text)
            ? all
            : all.Where(n => n.Contains(cb.Text, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!cb.IsDropDownOpen && !string.IsNullOrEmpty(cb.Text)) cb.IsDropDownOpen = true;
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
        => ItemRows.Add(new OutwardItemRow { Quantity = 1 });

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is OutwardItemRow row) ItemRows.Remove(row);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        try { ItemsGrid.CancelEdit(DataGridEditingUnit.Row); } catch { }
        base.OnClosing(e);
    }

    private void AccessoryCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb && cb.SelectedValue is string id &&
            ItemsGrid.SelectedItem is OutwardItemRow row)
            row.AccessoryId = id;
    }

    private void RecipientFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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

    private string GetRecipient() =>
        RecipientCombo.SelectedItem is Vendor v ? v.Name : RecipientCombo.Text?.Trim() ?? string.Empty;

    private string GetRecipientVendorId() =>
        RecipientCombo.SelectedItem is Vendor v ? v.VendorId : string.Empty;

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ErrorPanel.Visibility = Visibility.Collapsed;
            var recipient = GetRecipient();
            if (string.IsNullOrWhiteSpace(recipient))
            {
                MessageBox.Show("Recipient is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var style = StyleCombo.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(style))
            {
                if (!DataService.Instance.GetStyles().Any())
                    MessageBox.Show("No styles configured. Please add styles in Manage Add-On first.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Style is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                var outward = new OutwardOrder
                {
                    Recipient = recipient,
                    RecipientVendorId = GetRecipientVendorId(),
                    Purpose = PurposeBox.Text.Trim(),
                    Style = style,
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
                    AuditService.Instance.Log("ERROR", "OutwardOrder", $"Add failed: {error}");
                    return;
                }
            }
            else
            {
                var updatedOrder = JsonConvert.DeserializeObject<OutwardOrder>(
                    JsonConvert.SerializeObject(_existing))!;
                updatedOrder.Recipient = recipient;
                updatedOrder.RecipientVendorId = GetRecipientVendorId();
                updatedOrder.Purpose = PurposeBox.Text.Trim();
                updatedOrder.Style = style;
                updatedOrder.OutwardDate = OutwardDatePicker.SelectedDate ?? DateTime.Today;
                updatedOrder.Notes = NotesBox.Text.Trim();
                var newItems = ItemRows.Select(r => new OutwardOrderItem
                {
                    AccessoryId = r.AccessoryId, Quantity = r.Quantity
                }).ToList();
                foreach (var ni in newItems)
                {
                    var old = _existing!.Items.FirstOrDefault(i => i.AccessoryId == ni.AccessoryId);
                    if (old != null) { ni.ReturnedQuantity = old.ReturnedQuantity; ni.UsedQuantity = old.UsedQuantity; }
                }
                updatedOrder.Items = newItems;
                if (!DataService.Instance.UpdateOutwardOrder(updatedOrder, out var error))
                {
                    ErrorPanel.Visibility = Visibility.Visible;
                    ErrorText.Text = error;
                    AuditService.Instance.Log("ERROR", "OutwardOrder", $"Update failed: {error}");
                    return;
                }
            }
            DialogResult = true;
        }
        catch (Exception ex)
        {
            AuditService.Instance.Log("ERROR", "OutwardOrder", $"Save exception: {ex.Message}");
            MessageBox.Show($"Error saving outward order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
