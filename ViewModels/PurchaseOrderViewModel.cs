using System.Collections.ObjectModel;
using System.Windows;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class PurchaseOrderViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<PurchaseOrder> _orders = new();
    private ObservableCollection<PurchaseOrder> _filtered = new();
    private string _searchText = string.Empty;
    private POStatus? _filterStatus;
    private PurchaseOrder? _selected;

    public ObservableCollection<PurchaseOrder> Orders { get => _filtered; set => SetProperty(ref _filtered, value); }
    public PurchaseOrder? Selected { get => _selected; set => SetProperty(ref _selected, value); }
    public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }
    public POStatus? FilterStatus { get => _filterStatus; set { SetProperty(ref _filterStatus, value); ApplyFilter(); } }

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand ConvertCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ClearFilterCommand { get; }

    public PurchaseOrderViewModel()
    {
        AddCommand = new RelayCommand(_ => OpenAdd());
        EditCommand = new RelayCommand(_ => OpenEdit(), _ => Selected != null && Selected.Status == POStatus.Draft);
        ConvertCommand = new RelayCommand(_ => OpenConvert(), _ => Selected != null && (Selected.Status == POStatus.Draft || Selected.Status == POStatus.PartiallyInward));
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => Selected != null && Selected.Status == POStatus.Draft);
        RefreshCommand = new RelayCommand(_ => Refresh());
        ClearFilterCommand = new RelayCommand(_ => { FilterStatus = null; SearchText = string.Empty; });
        Refresh();
    }

    public void Refresh()
    {
        _orders = new ObservableCollection<PurchaseOrder>(
            DataService.Instance.GetPurchaseOrders().OrderByDescending(p => p.CreatedAt));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchText.ToLower();
        var result = _orders.Where(p =>
            (string.IsNullOrWhiteSpace(q) || p.PONumber.ToLower().Contains(q) ||
             DataService.Instance.GetVendorName(p.VendorId).ToLower().Contains(q)) &&
            (FilterStatus == null || p.Status == FilterStatus));
        Orders = new ObservableCollection<PurchaseOrder>(result);
    }

    public string GetVendorName(string id) => DataService.Instance.GetVendorName(id);

    private void OpenAdd()
    {
        var dlg = new TAM.Dialogs.PurchaseOrderDialog();
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void OpenEdit()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.PurchaseOrderDialog(Selected);
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void OpenConvert()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.InwardOrderDialog(Selected);
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void DeleteSelected()
    {
        if (Selected == null) return;
        if (MessageBox.Show($"Delete PO '{Selected.PONumber}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            DataService.Instance.DeletePurchaseOrder(Selected.POId);
            Refresh();
        }
    }
}
