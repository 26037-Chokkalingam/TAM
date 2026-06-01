using TAM.Models;
using TAM.Services;

namespace TAM.Helpers;

public static class DetailInfoBuilder
{
    public static DetailInfo ForVendor(Vendor v) => new()
    {
        Title = v.Name,
        Subtitle = v.VendorCode,
        Fields = new()
        {
            new() { Label = "Vendor Code", Value = v.VendorCode },
            new() { Label = "Name", Value = v.Name },
            new() { Label = "Contact Person", Value = v.ContactPerson },
            new() { Label = "Phone", Value = v.Phone },
            new() { Label = "Email", Value = v.Email },
            new() { Label = "Address", Value = v.Address },
            new() { Label = "Status", Value = v.IsActive ? "Active" : "Inactive" },
            new() { Label = "Created", Value = v.CreatedAt.ToString("dd-MM-yyyy HH:mm") },
            new() { Label = "Last Updated", Value = v.UpdatedAt.ToString("dd-MM-yyyy HH:mm") },
        },
        EditHistory = v.EditHistory
    };

    public static DetailInfo ForAccessory(Accessory a) => new()
    {
        Title = a.Name,
        Subtitle = a.AccessoryCode,
        Fields = new()
        {
            new() { Label = "Code", Value = a.AccessoryCode },
            new() { Label = "Name", Value = a.Name },
            new() { Label = "Category", Value = a.Category },
            new() { Label = "Unit", Value = a.Unit },
            new() { Label = "Current Stock", Value = $"{a.CurrentStock} {a.Unit}" },
            new() { Label = "Min Stock", Value = $"{a.MinimumStock} {a.Unit}" },
            new() { Label = "Stock Status", Value = a.IsLowStock ? "⚠ Low Stock" : "OK" },
            new() { Label = "Description", Value = a.Description },
            new() { Label = "Status", Value = a.IsActive ? "Active" : "Inactive" },
            new() { Label = "Created", Value = a.CreatedAt.ToString("dd-MM-yyyy HH:mm") },
        },
        EditHistory = a.EditHistory
    };

    public static DetailInfo ForPurchaseOrder(PurchaseOrder po) => new()
    {
        Title = po.PONumber,
        Subtitle = $"Vendor: {DataService.Instance.GetVendorName(po.VendorId)}",
        Fields = new()
        {
            new() { Label = "PO Number", Value = po.PONumber },
            new() { Label = "Vendor", Value = DataService.Instance.GetVendorName(po.VendorId) },
            new() { Label = "Status", Value = po.Status.ToString() },
            new() { Label = "Notes", Value = po.Notes },
            new() { Label = "Created", Value = po.CreatedAt.ToString("dd-MM-yyyy HH:mm") },
            new() { Label = "Last Updated", Value = po.UpdatedAt.ToString("dd-MM-yyyy HH:mm") },
        },
        ItemsHeader = "Order Items",
        ItemColumns = new() { "Accessory", "Requested", "Received", "" },
        Items = po.Items.Select(i => new DetailItemRow
        {
            Col1 = DataService.Instance.GetAccessoryName(i.AccessoryId),
            Col2 = i.RequestedQuantity.ToString("N2"),
            Col3 = i.ReceivedQuantity.ToString("N2"),
            Col4 = ""
        }).ToList(),
        EditHistory = po.EditHistory
    };

    public static DetailInfo ForInwardOrder(InwardOrder inw) => new()
    {
        Title = inw.InwardNumber,
        Subtitle = $"Bill No: {inw.BillNo}",
        Fields = new()
        {
            new() { Label = "Inward No.", Value = inw.InwardNumber },
            new() { Label = "Bill No.", Value = inw.BillNo },
            new() { Label = "Vendor", Value = DataService.Instance.GetVendorName(inw.VendorId) },
            new() { Label = "PO Reference", Value = string.IsNullOrEmpty(inw.POId) ? "Direct Entry" : $"From PO (ref: {inw.POId[..8]}...)" },
            new() { Label = "Inward Date", Value = inw.InwardDate.ToString("dd-MM-yyyy") },
            new() { Label = "Notes", Value = inw.Notes },
            new() { Label = "Created", Value = inw.CreatedAt.ToString("dd-MM-yyyy HH:mm") },
        },
        ItemsHeader = "Received Items",
        ItemColumns = new() { "Accessory", "Quantity", "", "" },
        Items = inw.Items.Select(i => new DetailItemRow
        {
            Col1 = DataService.Instance.GetAccessoryName(i.AccessoryId),
            Col2 = $"{i.Quantity:N2}",
            Col3 = "",
            Col4 = ""
        }).ToList(),
        EditHistory = inw.EditHistory
    };

    public static DetailInfo ForOutwardOrder(OutwardOrder out1) => new()
    {
        Title = out1.OutwardNumber,
        Subtitle = $"Recipient: {out1.Recipient}",
        Fields = new()
        {
            new() { Label = "Outward No.", Value = out1.OutwardNumber },
            new() { Label = "Recipient", Value = out1.Recipient },
            new() { Label = "Purpose", Value = out1.Purpose },
            new() { Label = "Status", Value = out1.Status.ToString() },
            new() { Label = "Outward Date", Value = out1.OutwardDate.ToString("dd-MM-yyyy") },
            new() { Label = "Notes", Value = out1.Notes },
            new() { Label = "Created", Value = out1.CreatedAt.ToString("dd-MM-yyyy HH:mm") },
        },
        ItemsHeader = "Dispatched Items",
        ItemColumns = new() { "Accessory", "Dispatched", "Returned", "Remaining" },
        Items = out1.Items.Select(i => new DetailItemRow
        {
            Col1 = DataService.Instance.GetAccessoryName(i.AccessoryId),
            Col2 = $"{i.Quantity:N2}",
            Col3 = $"{i.ReturnedQuantity:N2}",
            Col4 = $"{i.Quantity - i.ReturnedQuantity:N2}"
        }).ToList(),
        EditHistory = out1.EditHistory
    };

    public static DetailInfo ForReturnOrder(ReturnOrder ret) => new()
    {
        Title = ret.ReturnNumber,
        Subtitle = $"From: {DataService.Instance.GetOutwardById(ret.OutwardId)?.OutwardNumber ?? "-"}",
        Fields = new()
        {
            new() { Label = "Return No.", Value = ret.ReturnNumber },
            new() { Label = "Outward Order", Value = DataService.Instance.GetOutwardById(ret.OutwardId)?.OutwardNumber ?? "-" },
            new() { Label = "Return Type", Value = ret.IsFullReturn ? "Full Return" : "Partial Return" },
            new() { Label = "Return Date", Value = ret.ReturnDate.ToString("dd-MM-yyyy") },
            new() { Label = "Notes", Value = ret.Notes },
            new() { Label = "Created", Value = ret.CreatedAt.ToString("dd-MM-yyyy HH:mm") },
        },
        ItemsHeader = "Returned Items",
        ItemColumns = new() { "Accessory", "Returned Qty", "", "" },
        Items = ret.Items.Select(i => new DetailItemRow
        {
            Col1 = DataService.Instance.GetAccessoryName(i.AccessoryId),
            Col2 = $"{i.ReturnedQuantity:N2}",
            Col3 = i.Reason,
            Col4 = string.Empty
        }).ToList(),
        EditHistory = ret.EditHistory
    };
}
