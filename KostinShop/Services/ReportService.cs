using KostinShop.Data;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.IO;
using NPOI.XSSF.UserModel;

namespace KostinShop.Services;

public static class ReportService
{
    public static void GenerateSingleOrderReport(int orderId, string filePath)
    {
        using var db = DbContextFactory.Create();

        var order = db.Orders
            .Include(o => o.Client).ThenInclude(c => c.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.ProductOrders).ThenInclude(po => po.Product)
            .FirstOrDefault(o => o.ID_Order == orderId);

        if (order == null)
            throw new InvalidOperationException("Заказ не найден.");

        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet($"Заказ {orderId}");
        sheet.DefaultColumnWidth = 20;

        var headerStyle = CreateHeaderStyle(workbook);
        var dataStyle   = CreateDataStyle(workbook);
        var totalStyle  = CreateTotalStyle(workbook);

        var titleCell = sheet.CreateRow(0).CreateCell(0);
        titleCell.SetCellValue($"Заказ №{order.ID_Order} — KostinShop");
        titleCell.CellStyle = headerStyle;
        sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

        sheet.CreateRow(1).CreateCell(0)
             .SetCellValue($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");

        var infoRow = sheet.CreateRow(3);
        StyledCell(infoRow, 0, dataStyle).SetCellValue("Клиент");
        StyledCell(infoRow, 1, dataStyle).SetCellValue(ClientName(order.Client));
        StyledCell(infoRow, 2, dataStyle).SetCellValue("Статус");
        StyledCell(infoRow, 3, dataStyle).SetCellValue(order.OrderStatus?.Name ?? string.Empty);
        StyledCell(infoRow, 4, dataStyle).SetCellValue(order.Order_date.ToString("dd.MM.yyyy HH:mm"));

        var addressRow = sheet.CreateRow(4);
        StyledCell(addressRow, 0, dataStyle).SetCellValue("Адрес");
        StyledCell(addressRow, 1, dataStyle).SetCellValue(order.Delivery_Address);
        sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 4));

        string[] headers = ["Товар", "Цена", "Кол-во", "Сумма", "ID товара"]; 
        var headerRow = sheet.CreateRow(6);
        for (int i = 0; i < headers.Length; i++)
            StyledCell(headerRow, i, headerStyle).SetCellValue(headers[i]);

        int row = 7;
        decimal total = 0m;
        foreach (var item in order.ProductOrders)
        {
            var lineTotal = item.Quantity * item.Price_at_order;
            total += lineTotal;

            var r = sheet.CreateRow(row++);
            StyledCell(r, 0, dataStyle).SetCellValue(item.Product?.Name ?? item.ProductName);
            StyledCell(r, 1, dataStyle).SetCellValue((double)item.Price_at_order);
            StyledCell(r, 2, dataStyle).SetCellValue(item.Quantity);
            StyledCell(r, 3, dataStyle).SetCellValue((double)lineTotal);
            StyledCell(r, 4, dataStyle).SetCellValue(item.ID_Product);
        }

        var totalRow = sheet.CreateRow(row);
        StyledCell(totalRow, 2, totalStyle).SetCellValue("ИТОГО:");
        StyledCell(totalRow, 3, totalStyle).SetCellValue((double)total);

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        workbook.Write(fs);
    }

    /// <summary>
    /// Читает имя клиента напрямую из навигационного свойства User.
    /// Client.FullName не используется — его [NotMapped]-поля не заполняются EF.
    /// </summary>
    private static string ClientName(Models.Client client)
    {
        var u = client.User;
        if (u == null) return $"Клиент #{client.ID_Client}";
        return $"{u.Last_Name} {u.First_Name}".Trim();
    }

    private static ICell StyledCell(IRow row, int col, ICellStyle style)
    {
        var cell = row.CreateCell(col);
        cell.CellStyle = style;
        return cell;
    }

    private static ICellStyle CreateHeaderStyle(XSSFWorkbook wb)
    {
        var style = (XSSFCellStyle)wb.CreateCellStyle();
        var font  = (XSSFFont)wb.CreateFont();
        font.IsBold = true; font.FontName = "Arial";
        font.FontHeightInPoints = 11;
        font.SetColor(new XSSFColor(new byte[] { 255, 255, 255 }));
        style.SetFont(font);
        style.FillPattern = FillPattern.SolidForeground;
        ((XSSFCellStyle)style).SetFillForegroundColor(new XSSFColor(new byte[] { 181, 213, 202 }));
        style.BorderBottom = BorderStyle.Thin;
        style.Alignment = HorizontalAlignment.Center;
        return style;
    }

    private static ICellStyle CreateDataStyle(XSSFWorkbook wb)
    {
        var style = (XSSFCellStyle)wb.CreateCellStyle();
        var font  = (XSSFFont)wb.CreateFont();
        font.FontName = "Arial"; font.FontHeightInPoints = 10;
        font.SetColor(new XSSFColor(new byte[] { 0, 0, 0 }));
        style.SetFont(font);
        style.BorderBottom = BorderStyle.Hair;
        return style;
    }

    private static ICellStyle CreateAltDataStyle(XSSFWorkbook wb)
    {
        var style = (XSSFCellStyle)wb.CreateCellStyle();
        var font  = (XSSFFont)wb.CreateFont();
        font.FontName = "Arial"; font.FontHeightInPoints = 10;
        font.SetColor(new XSSFColor(new byte[] { 0, 0, 0 }));
        style.SetFont(font);
        style.FillPattern = FillPattern.SolidForeground;
        ((XSSFCellStyle)style).SetFillForegroundColor(new XSSFColor(new byte[] { 209, 238, 252 }));
        style.BorderBottom = BorderStyle.Hair;
        return style;
    }

    private static ICellStyle CreateTotalStyle(XSSFWorkbook wb)
    {
        var style = (XSSFCellStyle)wb.CreateCellStyle();
        var font  = (XSSFFont)wb.CreateFont();
        font.IsBold = true; font.FontName = "Arial";
        font.FontHeightInPoints = 11;
        font.SetColor(new XSSFColor(new byte[] { 0, 0, 0 }));
        style.SetFont(font);
        style.FillPattern = FillPattern.SolidForeground;
        ((XSSFCellStyle)style).SetFillForegroundColor(new XSSFColor(new byte[] { 255, 252, 214 }));
        style.BorderTop = BorderStyle.Medium;
        return style;
    }
}
