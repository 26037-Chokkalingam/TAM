using System.Collections.ObjectModel;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class VendorSummaryRow
{
    public string AccessoryCode { get; set; } = string.Empty;
    public string AccessoryName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal TotalBought { get; set; }
    public decimal TotalSent { get; set; }
    public decimal TotalReturned { get; set; }
    public decimal TotalUsed { get; set; }
    public decimal InHand => TotalSent - TotalReturned - TotalUsed;
}

public class VendorSummaryViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<Vendor> _allVendors = new();
    private ObservableCollection<VendorSummaryRow> _rows = new();
    private Vendor? _selectedVendor;

    public ObservableCollection<Vendor> AllVendors { get => _allVendors; set => SetProperty(ref _allVendors, value); }
    public ObservableCollection<VendorSummaryRow> Rows { get => _rows; set => SetProperty(ref _rows, value); }
    public Vendor? SelectedVendor
    {
        get => _selectedVendor;
        set { SetProperty(ref _selectedVendor, value); LoadSummary(); }
    }

    public RelayCommand RefreshCommand { get; }

    public VendorSummaryViewModel()
    {
        RefreshCommand = new RelayCommand(_ => Refresh());
        Refresh();
    }

    public void Refresh()
    {
        AllVendors = new ObservableCollection<Vendor>(
            DataService.Instance.GetVendors().Where(v => v.IsActive).OrderBy(v => v.Name));
        if (_selectedVendor != null)
        {
            var refreshed = AllVendors.FirstOrDefault(v => v.VendorId == _selectedVendor.VendorId);
            if (refreshed != null) _selectedVendor = refreshed;
        }
        LoadSummary();
    }

    private void LoadSummary()
    {
        if (_selectedVendor == null)
        {
            Rows = new ObservableCollection<VendorSummaryRow>();
            return;
        }

        var vendorId = _selectedVendor.VendorId;
        var vendorName = _selectedVendor.Name;
        var rowMap = new Dictionary<string, VendorSummaryRow>();

        void EnsureRow(string accId)
        {
            if (rowMap.ContainsKey(accId)) return;
            var acc = DataService.Instance.GetAccessoryById(accId);
            rowMap[accId] = new VendorSummaryRow
            {
                AccessoryCode = acc?.AccessoryCode ?? accId,
                AccessoryName = acc?.Name ?? accId,
                Unit = acc?.Unit ?? string.Empty
            };
        }

        // Bought: inward orders where VendorId = this vendor
        foreach (var inward in DataService.Instance.GetInwardOrders()
            .Where(i => i.VendorId == vendorId))
        {
            foreach (var item in inward.Items)
            {
                EnsureRow(item.AccessoryId);
                rowMap[item.AccessoryId].TotalBought += item.Quantity;
            }
        }

        // Sent + Used: outward orders where RecipientVendorId = vendorId OR Recipient matches name
        var outwardOrders = DataService.Instance.GetOutwardOrders()
            .Where(o => o.RecipientVendorId == vendorId ||
                        o.Recipient.Equals(vendorName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var outward in outwardOrders)
        {
            foreach (var item in outward.Items)
            {
                EnsureRow(item.AccessoryId);
                rowMap[item.AccessoryId].TotalSent += item.Quantity;
                rowMap[item.AccessoryId].TotalUsed += item.UsedQuantity;
            }
        }

        // Returned: return orders linked to those outward orders
        var outwardIds = outwardOrders.Select(o => o.OutwardId).ToHashSet();
        foreach (var ret in DataService.Instance.GetReturnOrders()
            .Where(r => outwardIds.Contains(r.OutwardId)))
        {
            foreach (var item in ret.Items)
            {
                EnsureRow(item.AccessoryId);
                rowMap[item.AccessoryId].TotalReturned += item.ReturnedQuantity;
            }
        }

        Rows = new ObservableCollection<VendorSummaryRow>(
            rowMap.Values
                .Where(r => r.TotalBought > 0 || r.TotalSent > 0)
                .OrderBy(r => r.AccessoryName));
    }
}
