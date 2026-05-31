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
    private InwardOrder? _selected;

    public ObservableCollection<InwardOrder> Orders { get => _filtered; set => SetProperty(ref _filtered, value); }
    public InwardOrder? Selected { get => _selected; set => SetProperty(ref _selected, value); }
    public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }

    public RelayCommand AddDirectCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ViewHistoryCommand { get; }

    public InwardOrderViewModel()
    {
        AddDirectCommand = new RelayCommand(_ => OpenAddDirect());
        EditCommand = new RelayCommand(_ => OpenEdit(), _ => Selected != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
        ViewHistoryCommand = new RelayCommand(_ => ViewHistory(), _ => Selected != null);
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
        var result = string.IsNullOrWhiteSpace(q)
            ? _orders
            : new ObservableCollection<InwardOrder>(_orders.Where(i =>
                i.InwardNumber.ToLower().Contains(q) || i.BillNo.ToLower().Contains(q) ||
                DataService.Instance.GetVendorName(i.VendorId).ToLower().Contains(q)));
        Orders = result;
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
