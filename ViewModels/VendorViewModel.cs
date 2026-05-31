using System.Collections.ObjectModel;
using System.Windows;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class VendorViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<Vendor> _vendors = new();
    private ObservableCollection<Vendor> _filtered = new();
    private string _searchText = string.Empty;
    private Vendor? _selected;

    public ObservableCollection<Vendor> Vendors { get => _filtered; set => SetProperty(ref _filtered, value); }
    public Vendor? Selected { get => _selected; set => SetProperty(ref _selected, value); }
    public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public VendorViewModel()
    {
        AddCommand = new RelayCommand(_ => OpenAdd());
        EditCommand = new RelayCommand(_ => OpenEdit(), _ => Selected != null);
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => Selected != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
        Refresh();
    }

    public void Refresh()
    {
        _vendors = new ObservableCollection<Vendor>(DataService.Instance.GetVendors().OrderBy(v => v.Name));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchText.ToLower();
        var result = string.IsNullOrWhiteSpace(q)
            ? _vendors
            : new ObservableCollection<Vendor>(_vendors.Where(v =>
                v.Name.ToLower().Contains(q) || v.VendorCode.ToLower().Contains(q) ||
                v.Phone.Contains(q) || v.Email.ToLower().Contains(q)));
        Vendors = result;
    }

    private void OpenAdd()
    {
        var dlg = new TAM.Dialogs.VendorDialog();
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void OpenEdit()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.VendorDialog(Selected);
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void DeleteSelected()
    {
        if (Selected == null) return;
        var r = MessageBox.Show($"Delete vendor '{Selected.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (r == MessageBoxResult.Yes)
        {
            DataService.Instance.DeleteVendor(Selected.VendorId);
            Refresh();
        }
    }
}
