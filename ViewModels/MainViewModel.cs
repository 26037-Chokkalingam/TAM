namespace TAM.ViewModels;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel _currentViewModel = null!;
    private string _activeNav = "Dashboard";

    public DashboardViewModel Dashboard { get; } = new();
    public VendorViewModel Vendors { get; } = new();
    public AccessoryViewModel Accessories { get; } = new();
    public PurchaseOrderViewModel PurchaseOrders { get; } = new();
    public InwardOrderViewModel InwardOrders { get; } = new();
    public OutwardOrderViewModel OutwardOrders { get; } = new();
    public ReturnOrderViewModel ReturnOrders { get; } = new();
    public SummaryViewModel Summary { get; } = new();
    public AuditLogViewModel AuditLog { get; } = new();
    public BackupViewModel Backup { get; } = new();

    public BaseViewModel CurrentViewModel { get => _currentViewModel; set => SetProperty(ref _currentViewModel, value); }
    public string ActiveNav { get => _activeNav; set => SetProperty(ref _activeNav, value); }

    public MainViewModel()
    {
        CurrentViewModel = Dashboard;
    }

    public void NavigateTo(string name)
    {
        ActiveNav = name;
        BaseViewModel next = name switch
        {
            "Dashboard" => Dashboard,
            "Vendors" => Vendors,
            "Accessories" => Accessories,
            "PurchaseOrders" => PurchaseOrders,
            "InwardOrders" => InwardOrders,
            "OutwardOrders" => OutwardOrders,
            "ReturnOrders" => ReturnOrders,
            "Summary" => Summary,
            "AuditLog" => AuditLog,
            "Backup" => Backup,
            _ => Dashboard
        };
        CurrentViewModel = next;
        if (next is IRefreshable r) r.Refresh();
    }
}
