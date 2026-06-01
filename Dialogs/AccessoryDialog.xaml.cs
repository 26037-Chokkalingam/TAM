using System.Windows;
using System.Windows.Controls;
using TAM.Models;
using TAM.Services;

namespace TAM.Dialogs;

public partial class AccessoryDialog : Window
{
    private Accessory? _existing;

    public AccessoryDialog(Accessory? accessory = null)
    {
        InitializeComponent();
        _existing = accessory;
        LoadCategories();

        if (accessory != null)
        {
            TitleText.Text = "Edit Accessory";
            NameBox.Text = accessory.Name;
            CategoryBox.Text = accessory.Category;
            UnitBox.Text = accessory.Unit;
            StockBox.Text = accessory.CurrentStock.ToString();
            MinStockBox.Text = accessory.MinimumStock.ToString();
            DescBox.Text = accessory.Description;
            ActiveCheck.IsChecked = accessory.IsActive;
        }
        else
        {
            UnitBox.Text = "pcs";
        }
    }

    private void LoadCategories()
    {
        var categories = DataService.Instance.GetCategories().Select(c => c.Name).ToList();
        CategoryBox.ItemsSource = categories;
    }

    private void CategoryFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not ComboBox cb) return;
        var all = DataService.Instance.GetCategories().Select(c => c.Name).ToList();
        if (cb.SelectedItem is string sel && sel == cb.Text) { cb.ItemsSource = all; return; }
        cb.ItemsSource = string.IsNullOrWhiteSpace(cb.Text)
            ? all
            : all.Where(n => n.Contains(cb.Text, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!cb.IsDropDownOpen && !string.IsNullOrEmpty(cb.Text)) cb.IsDropDownOpen = true;
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Accessory name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!DataService.Instance.IsAccessoryNameUnique(name, _existing?.AccessoryId))
        {
            MessageBox.Show($"Accessory '{name}' already exists. Name must be unique.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(StockBox.Text, out var stock))
        {
            MessageBox.Show("Invalid stock value.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(MinStockBox.Text, out var minStock))
        {
            MessageBox.Show("Invalid minimum stock value.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var category = CategoryBox.Text?.Trim() ?? string.Empty;

        if (_existing == null)
        {
            var a = new Accessory
            {
                Name = name,
                Category = category,
                Unit = UnitBox.Text.Trim(),
                CurrentStock = stock,
                MinimumStock = minStock,
                Description = DescBox.Text.Trim(),
                IsActive = ActiveCheck.IsChecked == true
            };
            DataService.Instance.AddAccessory(a);
        }
        else
        {
            _existing.Name = name;
            _existing.Category = category;
            _existing.Unit = UnitBox.Text.Trim();
            _existing.CurrentStock = stock;
            _existing.MinimumStock = minStock;
            _existing.Description = DescBox.Text.Trim();
            _existing.IsActive = ActiveCheck.IsChecked == true;
            DataService.Instance.UpdateAccessory(_existing, "Updated accessory details");
        }

        DialogResult = true;
    }
}
