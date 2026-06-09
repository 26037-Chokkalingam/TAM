using System.IO;
using Newtonsoft.Json;
using TAM.Models;

namespace TAM.Services;

public class DataService
{
    private static DataService? _instance;
    public static DataService Instance => _instance ??= new DataService();

    private readonly string _dataDir;
    private List<Vendor> _vendors = new();
    private List<Accessory> _accessories = new();
    private List<PurchaseOrder> _purchaseOrders = new();
    private List<InwardOrder> _inwardOrders = new();
    private List<OutwardOrder> _outwardOrders = new();
    private List<ReturnOrder> _returnOrders = new();
    private AddonData _addons = new();

    private DataService()
    {
        _dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TAM", "Data");
        Directory.CreateDirectory(_dataDir);
        LoadAll();
    }

    private void LoadAll()
    {
        _vendors = Load<List<Vendor>>("vendors.json") ?? new();
        _accessories = Load<List<Accessory>>("accessories.json") ?? new();
        _purchaseOrders = Load<List<PurchaseOrder>>("purchase_orders.json") ?? new();
        _inwardOrders = Load<List<InwardOrder>>("inward_orders.json") ?? new();
        _outwardOrders = Load<List<OutwardOrder>>("outward_orders.json") ?? new();
        _returnOrders = Load<List<ReturnOrder>>("return_orders.json") ?? new();
        _addons = Load<AddonData>("addons.json") ?? new();
        RecalculateAllStocks();
    }

    private void RecalculateAllStocks()
    {
        foreach (var acc in _accessories)
        {
            acc.CurrentStock =
                _inwardOrders.SelectMany(i => i.Items).Where(it => it.AccessoryId == acc.AccessoryId).Sum(it => it.Quantity)
                - _outwardOrders.SelectMany(o => o.Items).Where(it => it.AccessoryId == acc.AccessoryId).Sum(it => it.Quantity)
                + _returnOrders.SelectMany(r => r.Items).Where(it => it.AccessoryId == acc.AccessoryId).Sum(it => it.ReturnedQuantity);
        }
        Save("accessories.json", _accessories);
    }

    private T? Load<T>(string file) where T : class
    {
        var path = Path.Combine(_dataDir, file);
        if (!File.Exists(path)) return null;
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
    }

    private void Save<T>(string file, T data)
        => File.WriteAllText(Path.Combine(_dataDir, file), JsonConvert.SerializeObject(data, Formatting.Indented));

    private static EditHistoryEntry Snapshot<T>(T obj, string description) => new()
    {
        ChangeDescription = description,
        SnapshotJson = JsonConvert.SerializeObject(obj),
        ChangedAt = DateTime.Now
    };

    // ── VENDORS ─────────────────────────────────────────────────────────────

    public IReadOnlyList<Vendor> GetVendors() => _vendors.AsReadOnly();

    public void AddVendor(Vendor v)
    {
        v.VendorCode = $"VEN{(_vendors.Count + 1):D4}";
        _vendors.Add(v);
        Save("vendors.json", _vendors);
        AuditService.Instance.Log("CREATE", "Vendor", $"Added vendor: {v.Name}", v.VendorId);
    }

    public void UpdateVendor(Vendor v, string changeDesc = "Updated vendor")
    {
        var idx = _vendors.FindIndex(x => x.VendorId == v.VendorId);
        if (idx < 0) return;
        v.EditHistory.Insert(0, Snapshot(_vendors[idx], changeDesc));
        v.UpdatedAt = DateTime.Now;
        _vendors[idx] = v;
        Save("vendors.json", _vendors);
        AuditService.Instance.Log("UPDATE", "Vendor", $"{changeDesc}: {v.Name}", v.VendorId);
    }

    public void DeleteVendor(string id)
    {
        var v = _vendors.FirstOrDefault(x => x.VendorId == id);
        if (v == null) return;
        _vendors.Remove(v);
        Save("vendors.json", _vendors);
        AuditService.Instance.Log("DELETE", "Vendor", $"Deleted vendor: {v.Name}", id);
    }

    // ── ACCESSORIES ──────────────────────────────────────────────────────────

    public IReadOnlyList<Accessory> GetAccessories() => _accessories.AsReadOnly();

    public bool IsAccessoryNameUnique(string name, string? excludeId = null)
        => !_accessories.Any(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && a.AccessoryId != excludeId);

    public void AddAccessory(Accessory a)
    {
        a.AccessoryCode = $"ACC{(_accessories.Count + 1):D4}";
        _accessories.Add(a);
        Save("accessories.json", _accessories);
        AuditService.Instance.Log("CREATE", "Accessory", $"Added accessory: {a.Name}", a.AccessoryId);
    }

    public void UpdateAccessory(Accessory a, string changeDesc = "Updated accessory")
    {
        var idx = _accessories.FindIndex(x => x.AccessoryId == a.AccessoryId);
        if (idx < 0) return;
        a.EditHistory.Insert(0, Snapshot(_accessories[idx], changeDesc));
        a.UpdatedAt = DateTime.Now;
        _accessories[idx] = a;
        Save("accessories.json", _accessories);
        AuditService.Instance.Log("UPDATE", "Accessory", $"{changeDesc}: {a.Name}", a.AccessoryId);
    }

    public void DeleteAccessory(string id)
    {
        var a = _accessories.FirstOrDefault(x => x.AccessoryId == id);
        if (a == null) return;
        _accessories.Remove(a);
        Save("accessories.json", _accessories);
        AuditService.Instance.Log("DELETE", "Accessory", $"Deleted accessory: {a.Name}", id);
    }

    // ── PURCHASE ORDERS ──────────────────────────────────────────────────────

    public IReadOnlyList<PurchaseOrder> GetPurchaseOrders() => _purchaseOrders.AsReadOnly();

    public void AddPurchaseOrder(PurchaseOrder po)
    {
        po.PONumber = GeneratePONumber();
        _purchaseOrders.Add(po);
        Save("purchase_orders.json", _purchaseOrders);
        AuditService.Instance.Log("CREATE", "PurchaseOrder", $"Created PO: {po.PONumber}", po.POId);
    }

    public void UpdatePurchaseOrder(PurchaseOrder po, string changeDesc = "Updated PO")
    {
        var idx = _purchaseOrders.FindIndex(x => x.POId == po.POId);
        if (idx < 0) return;
        po.EditHistory.Insert(0, Snapshot(_purchaseOrders[idx], changeDesc));
        po.UpdatedAt = DateTime.Now;
        _purchaseOrders[idx] = po;
        Save("purchase_orders.json", _purchaseOrders);
        AuditService.Instance.Log("UPDATE", "PurchaseOrder", $"{changeDesc}: {po.PONumber}", po.POId);
    }

    public void DeletePurchaseOrder(string id)
    {
        var po = _purchaseOrders.FirstOrDefault(x => x.POId == id);
        if (po == null) return;
        _purchaseOrders.Remove(po);
        Save("purchase_orders.json", _purchaseOrders);
        AuditService.Instance.Log("DELETE", "PurchaseOrder", $"Deleted PO: {po.PONumber}", id);
    }

    // ── INWARD ORDERS ────────────────────────────────────────────────────────

    public IReadOnlyList<InwardOrder> GetInwardOrders() => _inwardOrders.AsReadOnly();

    public void AddInwardOrder(InwardOrder inward)
    {
        inward.InwardNumber = GenerateInwardNumber();
        ApplyInwardStock(inward.Items, +1);
        _inwardOrders.Add(inward);
        Save("inward_orders.json", _inwardOrders);
        Save("accessories.json", _accessories);
        AuditService.Instance.Log("CREATE", "InwardOrder", $"Created inward: {inward.InwardNumber} Bill:{inward.BillNo}", inward.InwardId);
    }

    public void UpdateInwardOrder(InwardOrder updated, string changeDesc = "Updated inward order")
    {
        var idx = _inwardOrders.FindIndex(x => x.InwardId == updated.InwardId);
        if (idx < 0) return;
        // Deep-copy old state immediately — dialog may have already mutated the in-memory object
        var oldJson = JsonConvert.SerializeObject(_inwardOrders[idx]);
        var oldState = JsonConvert.DeserializeObject<InwardOrder>(oldJson)!;
        // Reverse old stock, apply new stock using the captured old state
        ApplyInwardStock(oldState.Items, -1);
        ApplyInwardStock(updated.Items, +1);
        updated.EditHistory.Insert(0, new EditHistoryEntry { ChangeDescription = changeDesc, SnapshotJson = oldJson, ChangedAt = DateTime.Now });
        _inwardOrders[idx] = updated;
        Save("inward_orders.json", _inwardOrders);
        Save("accessories.json", _accessories);
        AuditService.Instance.Log("UPDATE", "InwardOrder", $"{changeDesc}: {updated.InwardNumber}", updated.InwardId);
    }

    private void ApplyInwardStock(List<InwardOrderItem> items, int sign)
    {
        foreach (var item in items)
        {
            var acc = _accessories.FirstOrDefault(a => a.AccessoryId == item.AccessoryId);
            if (acc != null) { acc.CurrentStock += sign * item.Quantity; acc.UpdatedAt = DateTime.Now; }
        }
    }

    public InwardOrder? ConvertPOToInward(PurchaseOrder po, Dictionary<string, decimal> received, string billNo, bool partial)
    {
        var inward = new InwardOrder { POId = po.POId, BillNo = billNo, VendorId = po.VendorId };
        foreach (var item in po.Items)
        {
            if (received.TryGetValue(item.ItemId, out var qty) && qty > 0)
            {
                inward.Items.Add(new InwardOrderItem { AccessoryId = item.AccessoryId, Quantity = qty });
                item.ReceivedQuantity += qty;
            }
        }
        if (!inward.Items.Any()) return null;

        var remaining = po.Items.Where(i => i.ReceivedQuantity < i.RequestedQuantity).ToList();
        if (partial && remaining.Any())
        {
            po.Status = POStatus.PartiallyInward;
            var newPO = new PurchaseOrder
            {
                VendorId = po.VendorId,
                Notes = $"Remaining from {po.PONumber}",
                Items = remaining.Select(r => new PurchaseOrderItem
                {
                    AccessoryId = r.AccessoryId,
                    RequestedQuantity = r.RequestedQuantity - r.ReceivedQuantity
                }).ToList()
            };
            AddPurchaseOrder(newPO);
        }
        else
        {
            po.Status = POStatus.Completed;
        }
        UpdatePurchaseOrder(po, "Converted to inward order");
        AddInwardOrder(inward);
        return inward;
    }

    // ── OUTWARD ORDERS ───────────────────────────────────────────────────────

    public IReadOnlyList<OutwardOrder> GetOutwardOrders() => _outwardOrders.AsReadOnly();

    public bool AddOutwardOrder(OutwardOrder outward, out string error)
    {
        error = string.Empty;
        foreach (var item in outward.Items)
        {
            var acc = _accessories.FirstOrDefault(a => a.AccessoryId == item.AccessoryId);
            if (acc == null) { error = "Accessory not found."; return false; }
            if (acc.CurrentStock < item.Quantity)
            {
                error = $"Insufficient stock for '{acc.Name}'. Available: {acc.CurrentStock}, Required: {item.Quantity}";
                return false;
            }
        }
        foreach (var item in outward.Items)
        {
            var acc = _accessories.First(a => a.AccessoryId == item.AccessoryId);
            acc.CurrentStock -= item.Quantity;
            acc.UpdatedAt = DateTime.Now;
        }
        outward.OutwardNumber = GenerateOutwardNumber();
        _outwardOrders.Add(outward);
        Save("outward_orders.json", _outwardOrders);
        Save("accessories.json", _accessories);
        AuditService.Instance.Log("CREATE", "OutwardOrder", $"Created outward: {outward.OutwardNumber}", outward.OutwardId);
        return true;
    }

    public bool UpdateOutwardOrder(OutwardOrder updated, out string error, string changeDesc = "Updated outward order")
    {
        error = string.Empty;
        var idx = _outwardOrders.FindIndex(x => x.OutwardId == updated.OutwardId);
        if (idx < 0) { error = "Order not found."; return false; }
        // Deep-copy old state immediately — dialog may have already mutated the in-memory object
        var oldJson = JsonConvert.SerializeObject(_outwardOrders[idx]);
        var oldState = JsonConvert.DeserializeObject<OutwardOrder>(oldJson)!;

        // Reverse old outward dispatch — returns are credited separately via CreateReturn, don't touch them here
        foreach (var item in oldState.Items)
        {
            var acc = _accessories.FirstOrDefault(a => a.AccessoryId == item.AccessoryId);
            if (acc != null) acc.CurrentStock += item.Quantity;
        }
        // Validate new items against restored stock
        foreach (var item in updated.Items)
        {
            var acc = _accessories.FirstOrDefault(a => a.AccessoryId == item.AccessoryId);
            if (acc == null) { RevertOutwardRestore(oldState); error = "Accessory not found."; return false; }
            if (acc.CurrentStock < item.Quantity)
            {
                RevertOutwardRestore(oldState);
                error = $"Insufficient stock for '{acc.Name}'. Available: {acc.CurrentStock}, Required: {item.Quantity}";
                return false;
            }
        }
        // Apply new stock
        foreach (var item in updated.Items)
        {
            var acc = _accessories.First(a => a.AccessoryId == item.AccessoryId);
            acc.CurrentStock -= item.Quantity;
            acc.UpdatedAt = DateTime.Now;
        }
        updated.EditHistory.Insert(0, new EditHistoryEntry { ChangeDescription = changeDesc, SnapshotJson = oldJson, ChangedAt = DateTime.Now });
        _outwardOrders[idx] = updated;
        Save("outward_orders.json", _outwardOrders);
        Save("accessories.json", _accessories);
        AuditService.Instance.Log("UPDATE", "OutwardOrder", $"{changeDesc}: {updated.OutwardNumber}", updated.OutwardId);
        return true;
    }

    private void RevertOutwardRestore(OutwardOrder oldState)
    {
        foreach (var item in oldState.Items)
        {
            var acc = _accessories.FirstOrDefault(a => a.AccessoryId == item.AccessoryId);
            if (acc != null) acc.CurrentStock -= item.Quantity;
        }
    }

    // ── RETURN ORDERS ────────────────────────────────────────────────────────

    public IReadOnlyList<ReturnOrder> GetReturnOrders() => _returnOrders.AsReadOnly();

    public ReturnOrder? CreateReturn(OutwardOrder outward, Dictionary<string, decimal> returnQtys, string notes, bool fullReturn)
    {
        var ret = new ReturnOrder
        {
            OutwardId = outward.OutwardId,
            IsFullReturn = fullReturn,
            Notes = notes,
            ReturnDate = DateTime.Now
        };

        foreach (var item in outward.Items)
        {
            if (!returnQtys.TryGetValue(item.ItemId, out var qty) || qty <= 0) continue;
            var remaining = item.Quantity - item.ReturnedQuantity;
            if (qty > remaining) qty = remaining;
            ret.Items.Add(new ReturnOrderItem { AccessoryId = item.AccessoryId, ReturnedQuantity = qty });
            item.ReturnedQuantity += qty;
            var acc = _accessories.FirstOrDefault(a => a.AccessoryId == item.AccessoryId);
            if (acc != null) { acc.CurrentStock += qty; acc.UpdatedAt = DateTime.Now; }
        }

        if (!ret.Items.Any()) return null;

        ret.ReturnNumber = GenerateReturnNumber();
        bool allReturned = outward.Items.All(i => i.ReturnedQuantity >= i.Quantity);
        outward.Status = allReturned ? OutwardOrderStatus.FullyReturned : OutwardOrderStatus.PartiallyReturned;

        var oidx = _outwardOrders.FindIndex(x => x.OutwardId == outward.OutwardId);
        if (oidx >= 0)
        {
            outward.EditHistory.Insert(0, Snapshot(_outwardOrders[oidx], "Return applied"));
            _outwardOrders[oidx] = outward;
        }

        _returnOrders.Add(ret);
        Save("return_orders.json", _returnOrders);
        Save("outward_orders.json", _outwardOrders);
        Save("accessories.json", _accessories);
        AuditService.Instance.Log("CREATE", "ReturnOrder", $"Return {ret.ReturnNumber} from {outward.OutwardNumber}", ret.ReturnId);
        return ret;
    }

    public void UpdateReturnOrder(ReturnOrder updated, string changeDesc = "Updated return order")
    {
        var idx = _returnOrders.FindIndex(x => x.ReturnId == updated.ReturnId);
        if (idx < 0) return;
        // Deep-copy old state immediately — dialog may have already mutated the in-memory object
        var oldJson = JsonConvert.SerializeObject(_returnOrders[idx]);
        var oldState = JsonConvert.DeserializeObject<ReturnOrder>(oldJson)!;

        // Reverse old return stock using captured old state, apply new
        foreach (var item in oldState.Items)
        {
            var acc = _accessories.FirstOrDefault(a => a.AccessoryId == item.AccessoryId);
            if (acc != null) { acc.CurrentStock -= item.ReturnedQuantity; acc.UpdatedAt = DateTime.Now; }
        }
        foreach (var item in updated.Items)
        {
            var acc = _accessories.FirstOrDefault(a => a.AccessoryId == item.AccessoryId);
            if (acc != null) { acc.CurrentStock += item.ReturnedQuantity; acc.UpdatedAt = DateTime.Now; }
        }

        // Recalculate outward returned quantities
        var outward = _outwardOrders.FirstOrDefault(o => o.OutwardId == updated.OutwardId);
        if (outward != null)
        {
            foreach (var oi in outward.Items) oi.ReturnedQuantity = 0;
            foreach (var allRet in _returnOrders.Where(r => r.ReturnId != updated.ReturnId && r.OutwardId == updated.OutwardId))
                foreach (var ri in allRet.Items)
                {
                    var oi = outward.Items.FirstOrDefault(i => i.AccessoryId == ri.AccessoryId);
                    if (oi != null) oi.ReturnedQuantity += ri.ReturnedQuantity;
                }
            foreach (var ri in updated.Items)
            {
                var oi = outward.Items.FirstOrDefault(i => i.AccessoryId == ri.AccessoryId);
                if (oi != null) oi.ReturnedQuantity += ri.ReturnedQuantity;
            }
            outward.Status = outward.Items.All(i => i.ReturnedQuantity >= i.Quantity)
                ? OutwardOrderStatus.FullyReturned
                : outward.Items.Any(i => i.ReturnedQuantity > 0)
                    ? OutwardOrderStatus.PartiallyReturned
                    : OutwardOrderStatus.Active;
            var oidx = _outwardOrders.FindIndex(x => x.OutwardId == outward.OutwardId);
            if (oidx >= 0) _outwardOrders[oidx] = outward;
        }

        updated.EditHistory.Insert(0, new EditHistoryEntry { ChangeDescription = changeDesc, SnapshotJson = oldJson, ChangedAt = DateTime.Now });
        _returnOrders[idx] = updated;
        Save("return_orders.json", _returnOrders);
        Save("outward_orders.json", _outwardOrders);
        Save("accessories.json", _accessories);
        AuditService.Instance.Log("UPDATE", "ReturnOrder", $"{changeDesc}: {updated.ReturnNumber}", updated.ReturnId);
    }

    // ── DASHBOARD ────────────────────────────────────────────────────────────

    public DashboardStats GetDashboardStats() => new()
    {
        TotalVendors = _vendors.Count(v => v.IsActive),
        TotalAccessories = _accessories.Count(a => a.IsActive),
        TotalPOs = _purchaseOrders.Count,
        PendingPOs = _purchaseOrders.Count(p => p.Status is POStatus.Draft or POStatus.PartiallyInward),
        TotalInwardOrders = _inwardOrders.Count,
        TotalOutwardOrders = _outwardOrders.Count,
        TotalReturnOrders = _returnOrders.Count,
        LowStockItems = _accessories.Count(a => a.IsActive && a.MinimumStock > 0 && a.CurrentStock <= a.MinimumStock),
        RecentInwards = _inwardOrders.OrderByDescending(i => i.CreatedAt).Take(5).ToList(),
        RecentOutwards = _outwardOrders.OrderByDescending(o => o.CreatedAt).Take(5).ToList()
    };

    // ── BACKUP / RESTORE ─────────────────────────────────────────────────────

    public AppData ExportData() => new()
    {
        Vendors = _vendors.ToList(),
        Accessories = _accessories.ToList(),
        PurchaseOrders = _purchaseOrders.ToList(),
        InwardOrders = _inwardOrders.ToList(),
        OutwardOrders = _outwardOrders.ToList(),
        ReturnOrders = _returnOrders.ToList()
    };

    public void ImportData(AppData data, bool clearExisting)
    {
        if (clearExisting) { _vendors.Clear(); _accessories.Clear(); _purchaseOrders.Clear(); _inwardOrders.Clear(); _outwardOrders.Clear(); _returnOrders.Clear(); }
        Merge(_vendors, data.Vendors, x => x.VendorId);
        Merge(_accessories, data.Accessories, x => x.AccessoryId);
        Merge(_purchaseOrders, data.PurchaseOrders, x => x.POId);
        Merge(_inwardOrders, data.InwardOrders, x => x.InwardId);
        Merge(_outwardOrders, data.OutwardOrders, x => x.OutwardId);
        Merge(_returnOrders, data.ReturnOrders, x => x.ReturnId);
        Save("vendors.json", _vendors); Save("accessories.json", _accessories);
        Save("purchase_orders.json", _purchaseOrders); Save("inward_orders.json", _inwardOrders);
        Save("outward_orders.json", _outwardOrders); Save("return_orders.json", _returnOrders);
        AuditService.Instance.Log("IMPORT", "System", $"Data imported (mode:{(clearExisting ? "replace" : "append")})");
    }

    private static void Merge<T>(List<T> target, List<T> source, Func<T, string> getId)
    {
        var existing = target.Select(getId).ToHashSet();
        foreach (var item in source)
            if (!existing.Contains(getId(item))) target.Add(item);
    }

    // ── ADD-ONS (Categories + Styles) ────────────────────────────────────────

    public IReadOnlyList<AddonItem> GetCategories() => _addons.Categories.AsReadOnly();
    public IReadOnlyList<AddonItem> GetStyles() => _addons.Styles.AsReadOnly();

    public bool IsCategoryNameUnique(string name, string? excludeId = null)
        => !_addons.Categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && c.Id != excludeId);

    public bool IsStyleNameUnique(string name, string? excludeId = null)
        => !_addons.Styles.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && s.Id != excludeId);

    public void AddCategory(AddonItem item)
    {
        _addons.Categories.Add(item);
        Save("addons.json", _addons);
    }

    public void UpdateCategory(AddonItem item)
    {
        var idx = _addons.Categories.FindIndex(c => c.Id == item.Id);
        if (idx >= 0) { _addons.Categories[idx] = item; Save("addons.json", _addons); }
    }

    public void DeleteCategory(string id)
    {
        _addons.Categories.RemoveAll(c => c.Id == id);
        Save("addons.json", _addons);
    }

    public void AddStyle(AddonItem item)
    {
        _addons.Styles.Add(item);
        Save("addons.json", _addons);
    }

    public void UpdateStyle(AddonItem item)
    {
        var idx = _addons.Styles.FindIndex(s => s.Id == item.Id);
        if (idx >= 0) { _addons.Styles[idx] = item; Save("addons.json", _addons); }
    }

    public void DeleteStyle(string id)
    {
        _addons.Styles.RemoveAll(s => s.Id == id);
        Save("addons.json", _addons);
    }

    // ── CLOSE OUTWARD ORDER ──────────────────────────────────────────────────

    public bool CloseOutwardOrder(OutwardOrder outward, Dictionary<string, decimal> usedQtys, out string error)
    {
        error = string.Empty;
        var idx = _outwardOrders.FindIndex(x => x.OutwardId == outward.OutwardId);
        if (idx < 0) { error = "Order not found."; return false; }

        foreach (var item in outward.Items)
        {
            var used = usedQtys.TryGetValue(item.ItemId, out var u) ? u : 0;
            if (used < 0) { error = "Used quantity cannot be negative."; return false; }
            if (used + item.ReturnedQuantity > item.Quantity)
            {
                var accName = GetAccessoryName(item.AccessoryId);
                error = $"Used + Returned ({used + item.ReturnedQuantity}) exceeds dispatched ({item.Quantity}) for '{accName}'.";
                return false;
            }
        }

        var oldJson = JsonConvert.SerializeObject(_outwardOrders[idx]);
        foreach (var item in outward.Items)
            item.UsedQuantity = usedQtys.TryGetValue(item.ItemId, out var u) ? u : 0;

        outward.Status = OutwardOrderStatus.Closed;
        outward.EditHistory.Insert(0, new EditHistoryEntry
        {
            ChangeDescription = "Order closed",
            SnapshotJson = oldJson,
            ChangedAt = DateTime.Now
        });
        _outwardOrders[idx] = outward;
        Save("outward_orders.json", _outwardOrders);
        AuditService.Instance.Log("UPDATE", "OutwardOrder", $"Closed outward order: {outward.OutwardNumber}", outward.OutwardId);
        return true;
    }

    // ── LOOKUPS ──────────────────────────────────────────────────────────────

    public Vendor? GetVendorById(string id) => _vendors.FirstOrDefault(v => v.VendorId == id);
    public Accessory? GetAccessoryById(string id) => _accessories.FirstOrDefault(a => a.AccessoryId == id);
    public string GetVendorName(string id) => _vendors.FirstOrDefault(v => v.VendorId == id)?.Name ?? "-";
    public string GetAccessoryName(string id) => _accessories.FirstOrDefault(a => a.AccessoryId == id)?.Name ?? "-";
    public OutwardOrder? GetOutwardById(string id) => _outwardOrders.FirstOrDefault(o => o.OutwardId == id);

    // ── NUMBER GENERATORS ────────────────────────────────────────────────────

    private string GeneratePONumber()
    {
        var y = DateTime.Now.Year;
        return $"PO-{y}-{(_purchaseOrders.Count(p => p.CreatedAt.Year == y) + 1):D4}";
    }
    private string GenerateInwardNumber()
    {
        var y = DateTime.Now.Year;
        return $"INW-{y}-{(_inwardOrders.Count(i => i.CreatedAt.Year == y) + 1):D4}";
    }
    private string GenerateOutwardNumber()
    {
        var y = DateTime.Now.Year;
        return $"OUT-{y}-{(_outwardOrders.Count(o => o.CreatedAt.Year == y) + 1):D4}";
    }
    private string GenerateReturnNumber()
    {
        var y = DateTime.Now.Year;
        return $"RET-{y}-{(_returnOrders.Count(r => r.CreatedAt.Year == y) + 1):D4}";
    }
}

public class DashboardStats
{
    public int TotalVendors { get; set; }
    public int TotalAccessories { get; set; }
    public int TotalPOs { get; set; }
    public int PendingPOs { get; set; }
    public int TotalInwardOrders { get; set; }
    public int TotalOutwardOrders { get; set; }
    public int TotalReturnOrders { get; set; }
    public int LowStockItems { get; set; }
    public List<InwardOrder> RecentInwards { get; set; } = new();
    public List<OutwardOrder> RecentOutwards { get; set; } = new();
}
