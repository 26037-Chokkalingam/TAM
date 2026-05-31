using System.Windows;
using TAM.Models;

namespace TAM.Dialogs;

public partial class ImportDialog : Window
{
    public bool ClearExisting => ReplaceRadio.IsChecked == true;

    public ImportDialog(AppData preview)
    {
        InitializeComponent();
        VendorCount.Text = preview.Vendors.Count.ToString();
        AccessoryCount.Text = preview.Accessories.Count.ToString();
        OrderCount.Text = $"PO:{preview.PurchaseOrders.Count} In:{preview.InwardOrders.Count} Out:{preview.OutwardOrders.Count} Ret:{preview.ReturnOrders.Count}";
        if (DateTime.TryParse(preview.ExportedAt, out var dt))
            ExportedAtText.Text = $"Exported at: {dt:dd-MM-yyyy HH:mm}";
        else
            ExportedAtText.Text = $"Exported at: {preview.ExportedAt}";
    }

    private void ImportBtn_Click(object sender, RoutedEventArgs e)
    {
        if (ReplaceRadio.IsChecked == true)
        {
            var confirm = MessageBox.Show(
                "This will DELETE all current data and replace it with the backup. Are you sure?",
                "Confirm Replace", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;
        }
        DialogResult = true;
    }
}
