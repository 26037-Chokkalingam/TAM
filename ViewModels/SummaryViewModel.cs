using System.Collections.ObjectModel;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class StockSummaryRow
{
    public string AccessoryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal TotalInward { get; set; }
    public decimal TotalOutward { get; set; }
    public decimal TotalReturned { get; set; }
    public bool IsLowStock => MinimumStock > 0 && CurrentStock <= MinimumStock;
    public string StockStatus => IsLowStock ? "Low Stock" : "OK";
    public List<string> VendorNames { get; set; } = new();
    public string VendorsSummary => string.Join(", ", VendorNames);
}

public class SummaryViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<StockSummaryRow> _rows = new();
    private ObservableCollection<StockSummaryRow> _filtered = new();
    private string _searchText = string.Empty;
    private string _filterVendor = string.Empty;
    private string _filterAccessory = string.Empty;

    public ObservableCollection<StockSummaryRow> Rows { get => _filtered; set => SetProperty(ref _filtered, value); }
    public ObservableCollection<string> VendorFilter { get; } = new();
    public ObservableCollection<string> AccessoryFilter { get; } = new();

    public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }
    public string FilterVendor { get => _filterVendor; set { SetProperty(ref _filterVendor, value); ApplyFilter(); } }
    public string FilterAccessory { get => _filterAccessory; set { SetProperty(ref _filterAccessory, value); ApplyFilter(); } }

    public RelayCommand ExportExcelCommand { get; }
    public RelayCommand ExportPdfCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ClearFilterCommand { get; }

    public SummaryViewModel()
    {
        ExportExcelCommand = new RelayCommand(_ => ExportExcel());
        ExportPdfCommand = new RelayCommand(_ => ExportPdf());
        RefreshCommand = new RelayCommand(_ => Refresh());
        ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; FilterVendor = string.Empty; FilterAccessory = string.Empty; });
        Refresh();
    }

    public void Refresh()
    {
        var accessories = DataService.Instance.GetAccessories().ToList();
        var inwards = DataService.Instance.GetInwardOrders().ToList();
        var outwards = DataService.Instance.GetOutwardOrders().ToList();
        var returns = DataService.Instance.GetReturnOrders().ToList();

        _rows.Clear();
        foreach (var a in accessories.Where(x => x.IsActive))
        {
            var totalIn = inwards.SelectMany(i => i.Items).Where(it => it.AccessoryId == a.AccessoryId).Sum(it => it.Quantity);
            var totalOut = outwards.SelectMany(o => o.Items).Where(it => it.AccessoryId == a.AccessoryId).Sum(it => it.Quantity);
            var totalRet = returns.SelectMany(r => r.Items).Where(it => it.AccessoryId == a.AccessoryId).Sum(it => it.ReturnedQuantity);
            var vendors = inwards.Where(i => i.Items.Any(it => it.AccessoryId == a.AccessoryId))
                .Select(i => DataService.Instance.GetVendorName(i.VendorId)).Distinct().ToList();
            _rows.Add(new StockSummaryRow
            {
                AccessoryCode = a.AccessoryCode, Name = a.Name, Category = a.Category,
                Unit = a.Unit, CurrentStock = a.CurrentStock, MinimumStock = a.MinimumStock,
                TotalInward = totalIn, TotalOutward = totalOut, TotalReturned = totalRet,
                VendorNames = vendors
            });
        }

        VendorFilter.Clear(); VendorFilter.Add("All");
        foreach (var v in DataService.Instance.GetVendors().Select(v => v.Name).OrderBy(n => n)) VendorFilter.Add(v);
        AccessoryFilter.Clear(); AccessoryFilter.Add("All");
        foreach (var a in accessories.Select(a => a.Name).OrderBy(n => n)) AccessoryFilter.Add(a);

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchText.ToLower();
        var result = _rows.Where(r =>
            (string.IsNullOrWhiteSpace(q) || r.Name.ToLower().Contains(q) || r.AccessoryCode.ToLower().Contains(q)) &&
            (string.IsNullOrEmpty(FilterVendor) || FilterVendor == "All" || r.VendorNames.Any(v => v == FilterVendor)) &&
            (string.IsNullOrEmpty(FilterAccessory) || FilterAccessory == "All" || r.Name == FilterAccessory));
        Rows = new ObservableCollection<StockSummaryRow>(result);
    }

    private void ExportExcel()
    {
        var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Excel|*.xlsx", FileName = $"StockReport_{DateTime.Now:yyyyMMdd}" };
        if (dlg.ShowDialog() == true)
        {
            ReportService.Instance.ExportStockReport(dlg.FileName);
            System.Windows.MessageBox.Show("Excel report exported successfully.", "Export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }

    private void ExportPdf()
    {
        var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "PDF|*.pdf", FileName = $"StockReport_{DateTime.Now:yyyyMMdd}" };
        if (dlg.ShowDialog() == true)
        {
            ReportService.Instance.ExportStockReportPdf(dlg.FileName);
            System.Windows.MessageBox.Show("PDF report exported successfully.", "Export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}
