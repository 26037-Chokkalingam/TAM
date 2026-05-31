using System.Collections.ObjectModel;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class ReturnOrderViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<ReturnOrder> _orders = new();
    private ObservableCollection<ReturnOrder> _filtered = new();
    private string _searchText = string.Empty;
    private ReturnOrder? _selected;

    public ObservableCollection<ReturnOrder> Orders { get => _filtered; set => SetProperty(ref _filtered, value); }
    public ReturnOrder? Selected { get => _selected; set => SetProperty(ref _selected, value); }
    public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }

    public RelayCommand EditCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ViewHistoryCommand { get; }

    public ReturnOrderViewModel()
    {
        EditCommand = new RelayCommand(_ => OpenEdit(), _ => Selected != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
        ViewHistoryCommand = new RelayCommand(_ => ViewHistory(), _ => Selected != null);
        Refresh();
    }

    public void Refresh()
    {
        _orders = new ObservableCollection<ReturnOrder>(
            DataService.Instance.GetReturnOrders().OrderByDescending(r => r.CreatedAt));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchText.ToLower();
        var result = string.IsNullOrWhiteSpace(q)
            ? _orders
            : new ObservableCollection<ReturnOrder>(_orders.Where(r =>
                r.ReturnNumber.ToLower().Contains(q) ||
                DataService.Instance.GetOutwardById(r.OutwardId)?.OutwardNumber.ToLower().Contains(q) == true));
        Orders = result;
    }

    public string GetOutwardNumber(string outwardId)
        => DataService.Instance.GetOutwardById(outwardId)?.OutwardNumber ?? "-";
    public string GetAccessoryName(string id) => DataService.Instance.GetAccessoryName(id);

    private void OpenEdit()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.ReturnOrderDialog(Selected);
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void ViewHistory()
    {
        if (Selected == null) return;
        var dlg = new TAM.Dialogs.EditHistoryDialog("Return Order Edit History", Selected.EditHistory);
        dlg.ShowDialog();
    }
}
