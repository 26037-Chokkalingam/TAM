using System.Collections.ObjectModel;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class DashboardViewModel : BaseViewModel, IRefreshable
{
    private readonly Action<string, string?> _navigate;

    private int _totalVendors, _totalAccessories, _totalPOs, _pendingPOs, _totalInward, _totalOutward, _totalReturns, _lowStock;
    private ObservableCollection<InwardOrder> _recentInwards = new();
    private ObservableCollection<OutwardOrder> _recentOutwards = new();

    public int TotalVendors { get => _totalVendors; set => SetProperty(ref _totalVendors, value); }
    public int TotalAccessories { get => _totalAccessories; set => SetProperty(ref _totalAccessories, value); }
    public int TotalPOs { get => _totalPOs; set => SetProperty(ref _totalPOs, value); }
    public int PendingPOs { get => _pendingPOs; set => SetProperty(ref _pendingPOs, value); }
    public int TotalInward { get => _totalInward; set => SetProperty(ref _totalInward, value); }
    public int TotalOutward { get => _totalOutward; set => SetProperty(ref _totalOutward, value); }
    public int TotalReturns { get => _totalReturns; set => SetProperty(ref _totalReturns, value); }
    public int LowStock { get => _lowStock; set => SetProperty(ref _lowStock, value); }
    public ObservableCollection<InwardOrder> RecentInwards { get => _recentInwards; set => SetProperty(ref _recentInwards, value); }
    public ObservableCollection<OutwardOrder> RecentOutwards { get => _recentOutwards; set => SetProperty(ref _recentOutwards, value); }

    // Navigation tile commands
    public RelayCommand NavVendorsCommand { get; }
    public RelayCommand NavAccessoriesCommand { get; }
    public RelayCommand NavPOCommand { get; }
    public RelayCommand NavPendingPOCommand { get; }
    public RelayCommand NavInwardCommand { get; }
    public RelayCommand NavOutwardCommand { get; }
    public RelayCommand NavReturnCommand { get; }
    public RelayCommand NavLowStockCommand { get; }

    public DashboardViewModel(Action<string, string?> navigate)
    {
        _navigate = navigate;
        NavVendorsCommand = new RelayCommand(() => _navigate("Vendors", null));
        NavAccessoriesCommand = new RelayCommand(() => _navigate("Summary", null));
        NavPOCommand = new RelayCommand(() => _navigate("PurchaseOrders", null));
        NavPendingPOCommand = new RelayCommand(() => _navigate("PurchaseOrders", "Pending"));
        NavInwardCommand = new RelayCommand(() => _navigate("InwardOrders", null));
        NavOutwardCommand = new RelayCommand(() => _navigate("OutwardOrders", null));
        NavReturnCommand = new RelayCommand(() => _navigate("ReturnOrders", null));
        NavLowStockCommand = new RelayCommand(() => _navigate("Summary", "LowStock"));
        Refresh();
    }

    public void Refresh()
    {
        var stats = DataService.Instance.GetDashboardStats();
        TotalVendors = stats.TotalVendors;
        TotalAccessories = stats.TotalAccessories;
        TotalPOs = stats.TotalPOs;
        PendingPOs = stats.PendingPOs;
        TotalInward = stats.TotalInwardOrders;
        TotalOutward = stats.TotalOutwardOrders;
        TotalReturns = stats.TotalReturnOrders;
        LowStock = stats.LowStockItems;
        RecentInwards = new ObservableCollection<InwardOrder>(stats.RecentInwards);
        RecentOutwards = new ObservableCollection<OutwardOrder>(stats.RecentOutwards);
    }
}
