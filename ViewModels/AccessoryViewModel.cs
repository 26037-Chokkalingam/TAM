using System.Collections.ObjectModel;
using System.Windows;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class AccessoryViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<Accessory> _accessories = new();
    private ObservableCollection<Accessory> _filtered = new();
    private string _searchText = string.Empty;
    private string _filterCategory = string.Empty;
    private Accessory? _selected;

    public ObservableCollection<Accessory> Accessories { get => _filtered; set => SetProperty(ref _filtered, value); }
    public ObservableCollection<string> Categories { get; } = new();
    public Accessory? Selected { get => _selected; set => SetProperty(ref _selected, value); }
    public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }
    public string FilterCategory { get => _filterCategory; set { SetProperty(ref _filterCategory, value); ApplyFilter(); } }

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public AccessoryViewModel()
    {
        AddCommand = new RelayCommand(_ => OpenAdd());
        EditCommand = new RelayCommand(_ => OpenEdit(), _ => Selected != null);
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => Selected != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
        Refresh();
    }

    public void Refresh()
    {
        _accessories = new ObservableCollection<Accessory>(DataService.Instance.GetAccessories().OrderBy(a => a.Name));
        Categories.Clear();
        Categories.Add("All");
        foreach (var c in _accessories.Select(a => a.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(c => c))
            Categories.Add(c);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchText.ToLower();
        var result = _accessories.Where(a =>
            (string.IsNullOrWhiteSpace(q) || a.Name.ToLower().Contains(q) || a.AccessoryCode.ToLower().Contains(q) || a.Category.ToLower().Contains(q)) &&
            (string.IsNullOrEmpty(FilterCategory) || FilterCategory == "All" || a.Category == FilterCategory));
        Accessories = new ObservableCollection<Accessory>(result);
    }

    private void OpenAdd()
    {
        var dlg = new TAM.Dialogs.AccessoryDialog();
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void OpenEdit()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.AccessoryDialog(Selected);
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void DeleteSelected()
    {
        if (Selected == null) return;
        var r = MessageBox.Show($"Delete accessory '{Selected.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (r == MessageBoxResult.Yes)
        {
            DataService.Instance.DeleteAccessory(Selected.AccessoryId);
            Refresh();
        }
    }
}
