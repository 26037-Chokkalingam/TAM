using System.Collections.ObjectModel;
using System.Windows;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class ManageAddonViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<AddonItem> _categories = new();
    private ObservableCollection<AddonItem> _filteredCategories = new();
    private ObservableCollection<AddonItem> _styles = new();
    private ObservableCollection<AddonItem> _filteredStyles = new();
    private string _categorySearch = string.Empty;
    private string _styleSearch = string.Empty;
    private AddonItem? _selectedCategory;
    private AddonItem? _selectedStyle;

    public ObservableCollection<AddonItem> Categories { get => _filteredCategories; set => SetProperty(ref _filteredCategories, value); }
    public ObservableCollection<AddonItem> Styles { get => _filteredStyles; set => SetProperty(ref _filteredStyles, value); }
    public AddonItem? SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
    public AddonItem? SelectedStyle { get => _selectedStyle; set => SetProperty(ref _selectedStyle, value); }
    public string CategorySearch { get => _categorySearch; set { SetProperty(ref _categorySearch, value); FilterCategories(); } }
    public string StyleSearch { get => _styleSearch; set { SetProperty(ref _styleSearch, value); FilterStyles(); } }

    public RelayCommand AddCategoryCommand { get; }
    public RelayCommand EditCategoryCommand { get; }
    public RelayCommand DeleteCategoryCommand { get; }
    public RelayCommand AddStyleCommand { get; }
    public RelayCommand EditStyleCommand { get; }
    public RelayCommand DeleteStyleCommand { get; }

    public ManageAddonViewModel()
    {
        AddCategoryCommand = new RelayCommand(_ => AddCategory());
        EditCategoryCommand = new RelayCommand(_ => EditCategory(), _ => SelectedCategory != null);
        DeleteCategoryCommand = new RelayCommand(_ => DeleteCategory(), _ => SelectedCategory != null);
        AddStyleCommand = new RelayCommand(_ => AddStyle());
        EditStyleCommand = new RelayCommand(_ => EditStyle(), _ => SelectedStyle != null);
        DeleteStyleCommand = new RelayCommand(_ => DeleteStyle(), _ => SelectedStyle != null);
        Refresh();
    }

    public void Refresh()
    {
        _categories = new ObservableCollection<AddonItem>(
            DataService.Instance.GetCategories().OrderBy(c => c.Name));
        _styles = new ObservableCollection<AddonItem>(
            DataService.Instance.GetStyles().OrderBy(s => s.Name));
        FilterCategories();
        FilterStyles();
    }

    private void FilterCategories()
    {
        var q = CategorySearch.ToLower();
        Categories = new ObservableCollection<AddonItem>(
            _categories.Where(c => string.IsNullOrWhiteSpace(q) || c.Name.ToLower().Contains(q)));
    }

    private void FilterStyles()
    {
        var q = StyleSearch.ToLower();
        Styles = new ObservableCollection<AddonItem>(
            _styles.Where(s => string.IsNullOrWhiteSpace(q) || s.Name.ToLower().Contains(q)));
    }

    private void AddCategory()
    {
        var dlg = new TAM.Dialogs.AddonItemDialog("Add Category", "Category Name");
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ItemName)) return;
        var name = dlg.ItemName.Trim();
        if (!DataService.Instance.IsCategoryNameUnique(name, null))
        {
            MessageBox.Show($"Category '{name}' already exists.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DataService.Instance.AddCategory(new AddonItem { Name = name });
        Refresh();
    }

    private void EditCategory()
    {
        if (SelectedCategory == null) return;
        var dlg = new TAM.Dialogs.AddonItemDialog("Edit Category", "Category Name", SelectedCategory.Name);
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ItemName)) return;
        var name = dlg.ItemName.Trim();
        if (!DataService.Instance.IsCategoryNameUnique(name, SelectedCategory.Id))
        {
            MessageBox.Show($"Category '{name}' already exists.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        SelectedCategory.Name = name;
        DataService.Instance.UpdateCategory(SelectedCategory);
        Refresh();
    }

    private void DeleteCategory()
    {
        if (SelectedCategory == null) return;
        if (MessageBox.Show($"Delete category '{SelectedCategory.Name}'?", "Confirm",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            DataService.Instance.DeleteCategory(SelectedCategory.Id);
            Refresh();
        }
    }

    private void AddStyle()
    {
        var dlg = new TAM.Dialogs.AddonItemDialog("Add Style", "Style Name");
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ItemName)) return;
        var name = dlg.ItemName.Trim();
        if (!DataService.Instance.IsStyleNameUnique(name, null))
        {
            MessageBox.Show($"Style '{name}' already exists.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DataService.Instance.AddStyle(new AddonItem { Name = name });
        Refresh();
    }

    private void EditStyle()
    {
        if (SelectedStyle == null) return;
        var dlg = new TAM.Dialogs.AddonItemDialog("Edit Style", "Style Name", SelectedStyle.Name);
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.ItemName)) return;
        var name = dlg.ItemName.Trim();
        if (!DataService.Instance.IsStyleNameUnique(name, SelectedStyle.Id))
        {
            MessageBox.Show($"Style '{name}' already exists.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        SelectedStyle.Name = name;
        DataService.Instance.UpdateStyle(SelectedStyle);
        Refresh();
    }

    private void DeleteStyle()
    {
        if (SelectedStyle == null) return;
        if (MessageBox.Show($"Delete style '{SelectedStyle.Name}'?", "Confirm",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            DataService.Instance.DeleteStyle(SelectedStyle.Id);
            Refresh();
        }
    }
}
