using ClosedXML.Excel;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using TAM.Models;

namespace TAM.Services;

public class ReportService
{
    public static ReportService Instance { get; } = new();

    // ── EXCEL ────────────────────────────────────────────────────────────────

    public void ExportStockReport(string filePath)
    {
        var accessories = DataService.Instance.GetAccessories().ToList();
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Stock Report");
        SetExcelHeader(ws, new[] { "Code", "Name", "Category", "Unit", "Current Stock", "Min Stock", "Status" });
        int row = 2;
        foreach (var a in accessories.Where(a => a.IsActive))
        {
            ws.Cell(row, 1).Value = a.AccessoryCode;
            ws.Cell(row, 2).Value = a.Name;
            ws.Cell(row, 3).Value = a.Category;
            ws.Cell(row, 4).Value = a.Unit;
            ws.Cell(row, 5).Value = (double)a.CurrentStock;
            ws.Cell(row, 6).Value = (double)a.MinimumStock;
            ws.Cell(row, 7).Value = a.MinimumStock > 0 && a.CurrentStock <= a.MinimumStock ? "Low Stock" : "OK";
            if (a.MinimumStock > 0 && a.CurrentStock <= a.MinimumStock)
                ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.LightSalmon;
            row++;
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    public void ExportPurchaseOrderReport(string filePath)
    {
        var orders = DataService.Instance.GetPurchaseOrders().ToList();
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Purchase Orders");
        SetExcelHeader(ws, new[] { "PO Number", "Vendor", "Status", "Items", "Created Date" });
        int row = 2;
        foreach (var po in orders)
        {
            ws.Cell(row, 1).Value = po.PONumber;
            ws.Cell(row, 2).Value = DataService.Instance.GetVendorName(po.VendorId);
            ws.Cell(row, 3).Value = po.Status.ToString();
            ws.Cell(row, 4).Value = po.Items.Count;
            ws.Cell(row, 5).Value = po.CreatedAt.ToString("dd-MM-yyyy");
            row++;
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    public void ExportInwardReport(string filePath)
    {
        var orders = DataService.Instance.GetInwardOrders().ToList();
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Inward Orders");
        SetExcelHeader(ws, new[] { "Inward No", "Bill No", "Vendor", "Accessory", "Quantity", "Unit Price", "Date" });
        int row = 2;
        foreach (var inw in orders)
        {
            foreach (var item in inw.Items)
            {
                ws.Cell(row, 1).Value = inw.InwardNumber;
                ws.Cell(row, 2).Value = inw.BillNo;
                ws.Cell(row, 3).Value = DataService.Instance.GetVendorName(inw.VendorId);
                ws.Cell(row, 4).Value = DataService.Instance.GetAccessoryName(item.AccessoryId);
                ws.Cell(row, 5).Value = (double)item.Quantity;
                ws.Cell(row, 6).Value = (double)item.UnitPrice;
                ws.Cell(row, 7).Value = inw.InwardDate.ToString("dd-MM-yyyy");
                row++;
            }
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    public void ExportOutwardReport(string filePath)
    {
        var orders = DataService.Instance.GetOutwardOrders().ToList();
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Outward Orders");
        SetExcelHeader(ws, new[] { "Outward No", "Recipient", "Purpose", "Accessory", "Quantity", "Returned", "Status", "Date" });
        int row = 2;
        foreach (var out1 in orders)
        {
            foreach (var item in out1.Items)
            {
                ws.Cell(row, 1).Value = out1.OutwardNumber;
                ws.Cell(row, 2).Value = out1.Recipient;
                ws.Cell(row, 3).Value = out1.Purpose;
                ws.Cell(row, 4).Value = DataService.Instance.GetAccessoryName(item.AccessoryId);
                ws.Cell(row, 5).Value = (double)item.Quantity;
                ws.Cell(row, 6).Value = (double)item.ReturnedQuantity;
                ws.Cell(row, 7).Value = out1.Status.ToString();
                ws.Cell(row, 8).Value = out1.OutwardDate.ToString("dd-MM-yyyy");
                row++;
            }
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    private static void SetExcelHeader(IXLWorksheet ws, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#AD1457");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }

    // ── PDF ──────────────────────────────────────────────────────────────────

    public void ExportStockReportPdf(string filePath)
    {
        var accessories = DataService.Instance.GetAccessories().Where(a => a.IsActive).ToList();
        using var doc = new PdfDocument();
        doc.Info.Title = "TAM Stock Report";
        var page = doc.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;
        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
        var gfx = XGraphics.FromPdfPage(page);
        var fontBold = new XFont("Arial", 10, XFontStyle.Bold);
        var fontReg = new XFont("Arial", 9, XFontStyle.Regular);
        var titleFont = new XFont("Arial", 14, XFontStyle.Bold);
        double margin = 30, y = margin;

        gfx.DrawString("TAM - Stock Report", titleFont, XBrushes.DarkRed, margin, y);
        y += 20;
        gfx.DrawString($"Generated: {DateTime.Now:dd-MM-yyyy HH:mm}", fontReg, XBrushes.Gray, margin, y);
        y += 20;

        double[] cols = { 80, 160, 90, 60, 80, 70, 70 };
        string[] hdrs = { "Code", "Name", "Category", "Unit", "Stock", "Min Stock", "Status" };
        DrawPdfTableHeader(gfx, fontBold, hdrs, cols, margin, y);
        y += 18;

        foreach (var a in accessories)
        {
            if (y > page.Height - 40) { page = doc.AddPage(); gfx = XGraphics.FromPdfPage(page); y = margin; DrawPdfTableHeader(gfx, fontBold, hdrs, cols, margin, y); y += 18; }
            string[] vals = { a.AccessoryCode, a.Name, a.Category, a.Unit, a.CurrentStock.ToString("N2"), a.MinimumStock.ToString("N2"), a.MinimumStock > 0 && a.CurrentStock <= a.MinimumStock ? "Low!" : "OK" };
            var rowBrush = a.MinimumStock > 0 && a.CurrentStock <= a.MinimumStock ? XBrushes.LightSalmon : XBrushes.White;
            DrawPdfTableRow(gfx, fontReg, vals, cols, margin, y, rowBrush);
            y += 16;
        }
        doc.Save(filePath);
    }

    private static void DrawPdfTableHeader(XGraphics gfx, XFont font, string[] headers, double[] cols, double x, double y)
    {
        double cx = x;
        var hdrBrush = new XSolidBrush(XColor.FromArgb(255, 173, 20, 87));
        foreach (var (h, w) in headers.Zip(cols))
        {
            gfx.DrawRectangle(hdrBrush, cx, y - 12, w, 16);
            gfx.DrawString(h, font, XBrushes.White, new XRect(cx, y - 12, w, 16), XStringFormats.Center);
            cx += w;
        }
    }

    private static void DrawPdfTableRow(XGraphics gfx, XFont font, string[] values, double[] cols, double x, double y, XBrush bg)
    {
        double cx = x;
        foreach (var (v, w) in values.Zip(cols))
        {
            gfx.DrawRectangle(bg, cx, y - 12, w, 16);
            gfx.DrawRectangle(XPens.LightGray, cx, y - 12, w, 16);
            gfx.DrawString(v, font, XBrushes.Black, new XRect(cx + 2, y - 12, w - 4, 16), XStringFormats.CenterLeft);
            cx += w;
        }
    }
}
