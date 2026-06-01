using System.Collections.ObjectModel;
using System.Windows;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class InwardOrderViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<InwardOrder> _orders = new();
    private ObservableCollection<InwardOrder> _filtered = new();
    private string _searchText = string.Empty;
    private DateTime? _filterDateFrom;
    private DateTime? _filterDateTo;
    private InwardOrder? _selected;

    public ObservableCollection<InwardOrder> Orders { get => _filtered; set => SetProperty(ref _filtered, value); }
    public InwardOrder? Selected { get => _selected; set => SetProperty(ref _selected, value); }
    public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }
    public DateTime? FilterDateFrom { get => _filterDateFrom; set { SetProperty(ref _filterDateFrom, value); ApplyFilter(); } }
    public DateTime? FilterDateTo { get => _filterDateTo; set { SetProperty(ref _filterDateTo, value); ApplyFilter(); } }

    public RelayCommand AddDirectCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ViewHistoryCommand { get; }
    public RelayCommand ClearFilterCommand { get; }

    public InwardOrderViewModel()
    {
        AddDirectCommand = new RelayCommand(_ => OpenAddDirect());
        EditCommand = new RelayCommand(_ => OpenEdit(), _ => Selected != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
        ViewHistoryCommand = new RelayCommand(_ => ViewHistory(), _ => Selected != null);
        ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; FilterDateFrom = null; FilterDateTo = null; });
        Refresh();
    }

    public void Refresh()
    {
        _orders = new ObservableCollection<InwardOrder>(
            DataService.Instance.GetInwardOrders().OrderByDescending(i => i.CreatedAt));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchText.ToLower();
        var result = _orders.Where(i =>
            (string.IsNullOrWhiteSpace(q) || i.InwardNumber.ToLower().Contains(q) ||
             i.BillNo.ToLower().Contains(q) ||
             DataService.Instance.GetVendorName(i.VendorId).ToLower().Contains(q)) &&
            (FilterDateFrom == null || i.InwardDate.Date >= FilterDateFrom.Value.Date) &&
            (FilterDateTo == null || i.InwardDate.Date <= FilterDateTo.Value.Date));
        Orders = new ObservableCollection<InwardOrder>(result);
    }

    public string GetVendorName(string id) => DataService.Instance.GetVendorName(id);
    public string GetAccessoryName(string id) => DataService.Instance.GetAccessoryName(id);
    public string SummarizePORef(InwardOrder i) => string.IsNullOrEmpty(i.POId) ? "Direct" : $"From PO";
    public string SummarizeItems(InwardOrder i) => $"{i.Items.Count} item(s)";

    private void OpenAddDirect()
    {
        var dlg = new TAM.Dialogs.InwardOrderDialog();
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void OpenEdit()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.InwardOrderDialog(Selected);
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void ViewHistory()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.EditHistoryDialog("Inward Order Edit History", Selected.EditHistory);
        dlg.ShowDialog();
    }
}
