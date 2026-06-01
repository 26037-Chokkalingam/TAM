namespace TAM.ViewModels;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel _currentViewModel = null!;
    private string _activeNav = "Dashboard";

    public DashboardViewModel Dashboard { get; }
    public VendorViewModel Vendors { get; } = new();
    public AccessoryViewModel Accessories { get; } = new();
    public PurchaseOrderViewModel PurchaseOrders { get; } = new();
    public InwardOrderViewModel InwardOrders { get; } = new();
    public OutwardOrderViewModel OutwardOrders { get; } = new();
    public ReturnOrderViewModel ReturnOrders { get; } = new();
    public SummaryViewModel Summary { get; } = new();
    public AuditLogViewModel AuditLog { get; } = new();
    public BackupViewModel Backup { get; } = new();
    public ManageAddonViewModel ManageAddon { get; } = new();
    public VendorSummaryViewModel VendorSummary { get; } = new();

    public BaseViewModel CurrentViewModel { get => _currentViewModel; set => SetProperty(ref _currentViewModel, value); }
    public string ActiveNav { get => _activeNav; set => SetProperty(ref _activeNav, value); }

    public MainViewModel()
    {
        Dashboard = new DashboardViewModel(NavigateTo);
        CurrentViewModel = Dashboard;
    }

    public void NavigateTo(string name, string? param = null)
    {
        ActiveNav = name;
        switch (name)
        {
            case "Vendors":
                Vendors.Refresh();
                CurrentViewModel = Vendors;
                break;
            case "Accessories":
                Accessories.Refresh();
                CurrentViewModel = Accessories;
                break;
            case "PurchaseOrders":
                PurchaseOrders.Refresh();
                if (param == "Pending") PurchaseOrders.FilterStatus = TAM.Models.POStatus.Draft;
                CurrentViewModel = PurchaseOrders;
                break;
            case "InwardOrders":
                InwardOrders.Refresh();
                CurrentViewModel = InwardOrders;
                break;
            case "OutwardOrders":
                OutwardOrders.Refresh();
                CurrentViewModel = OutwardOrders;
                break;
            case "ReturnOrders":
                ReturnOrders.Refresh();
                CurrentViewModel = ReturnOrders;
                break;
            case "Summary":
                Summary.ShowLowStockOnly = param == "LowStock";
                Summary.Refresh();
                CurrentViewModel = Summary;
                break;
            case "AuditLog":
                AuditLog.Refresh();
                CurrentViewModel = AuditLog;
                break;
            case "Backup":
                CurrentViewModel = Backup;
                break;
            case "ManageAddon":
                ManageAddon.Refresh();
                CurrentViewModel = ManageAddon;
                break;
            case "VendorSummary":
                VendorSummary.Refresh();
                CurrentViewModel = VendorSummary;
                break;
            default:
                Dashboard.Refresh();
                CurrentViewModel = Dashboard;
                break;
        }
    }

    // Keep original overload for sidebar navigation
    public void NavigateTo(string name) => NavigateTo(name, null);
}
