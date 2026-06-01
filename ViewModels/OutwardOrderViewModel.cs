using System.Collections.ObjectModel;
using System.Windows;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class OutwardOrderViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<OutwardOrder> _orders = new();
    private ObservableCollection<OutwardOrder> _filtered = new();
    private string _searchText = string.Empty;
    private OutwardOrderStatus? _filterStatus;
    private DateTime? _filterDateFrom;
    private DateTime? _filterDateTo;
    private OutwardOrder? _selected;

    public ObservableCollection<OutwardOrder> Orders { get => _filtered; set => SetProperty(ref _filtered, value); }
    public OutwardOrder? Selected { get => _selected; set => SetProperty(ref _selected, value); }
    public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }
    public OutwardOrderStatus? FilterStatus { get => _filterStatus; set { SetProperty(ref _filterStatus, value); ApplyFilter(); } }
    public DateTime? FilterDateFrom { get => _filterDateFrom; set { SetProperty(ref _filterDateFrom, value); ApplyFilter(); } }
    public DateTime? FilterDateTo { get => _filterDateTo; set { SetProperty(ref _filterDateTo, value); ApplyFilter(); } }

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand ReturnCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ViewHistoryCommand { get; }
    public RelayCommand ClearFilterCommand { get; }

    public OutwardOrderViewModel()
    {
        AddCommand = new RelayCommand(_ => OpenAdd());
        EditCommand = new RelayCommand(_ => OpenEdit(), _ => Selected != null);
        ReturnCommand = new RelayCommand(_ => OpenReturn(), _ => Selected != null && Selected.Status != OutwardOrderStatus.FullyReturned);
        RefreshCommand = new RelayCommand(_ => Refresh());
        ViewHistoryCommand = new RelayCommand(_ => ViewHistory(), _ => Selected != null);
        ClearFilterCommand = new RelayCommand(_ => { FilterStatus = null; SearchText = string.Empty; FilterDateFrom = null; FilterDateTo = null; });
        Refresh();
    }

    public void Refresh()
    {
        _orders = new ObservableCollection<OutwardOrder>(
            DataService.Instance.GetOutwardOrders().OrderByDescending(o => o.CreatedAt));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchText.ToLower();
        var result = _orders.Where(o =>
            (string.IsNullOrWhiteSpace(q) || o.OutwardNumber.ToLower().Contains(q) ||
             o.Recipient.ToLower().Contains(q) || o.Purpose.ToLower().Contains(q)) &&
            (FilterStatus == null || o.Status == FilterStatus) &&
            (FilterDateFrom == null || o.OutwardDate.Date >= FilterDateFrom.Value.Date) &&
            (FilterDateTo == null || o.OutwardDate.Date <= FilterDateTo.Value.Date));
        Orders = new ObservableCollection<OutwardOrder>(result);
    }

    public string GetAccessoryName(string id) => DataService.Instance.GetAccessoryName(id);

    private void OpenAdd()
    {
        var dlg = new TAM.Dialogs.OutwardOrderDialog();
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void OpenEdit()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.OutwardOrderDialog(Selected);
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void OpenReturn()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.ReturnOrderDialog(Selected);
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void ViewHistory()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.EditHistoryDialog("Outward Order Edit History", Selected.EditHistory);
        dlg.ShowDialog();
    }
}
