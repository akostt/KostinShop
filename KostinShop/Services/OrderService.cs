using KostinShop.Data;
using KostinShop.Models;
using Microsoft.EntityFrameworkCore;

namespace KostinShop.Services;

public class OrderSummaryDto
{
    public int      ID_Order         { get; set; }
    public DateTime Order_date       { get; set; }
    public int      ID_Client        { get; set; }
    public string   ClientFullName   { get; set; } = string.Empty;
    public string   Status           { get; set; } = string.Empty;
    public string   Delivery_Address { get; set; } = string.Empty;
    public decimal  OrderTotal       { get; set; }
    public int      ItemCount        { get; set; }
    public int      ID_Order_Status  { get; set; }
}

public static class OrderService
{
    private static IQueryable<Order> WithDetails(ShopDbContext db) =>
        db.Orders
            .Include(o => o.Client).ThenInclude(c => c.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.ProductOrders);

    private static OrderSummaryDto ToDto(Order o) => new()
    {
        ID_Order         = o.ID_Order,
        Order_date       = o.Order_date,
        ID_Client        = o.ID_Client,
        ClientFullName   = o.Client?.User?.FullName ?? o.Client?.FullName ?? $"[Клиент #{o.ID_Client}]",
        Status           = o.OrderStatus?.Name ?? $"[Статус #{o.ID_Order_Status}]",
        Delivery_Address = o.Delivery_Address,
        OrderTotal       = o.ProductOrders?.Sum(po => po.Quantity * po.Price_at_order) ?? 0m,
        ItemCount        = o.ProductOrders?.Sum(po => po.Quantity) ?? 0,
        ID_Order_Status  = o.ID_Order_Status
    };

    public static List<OrderSummaryDto> GetSummary(int? statusId = null)
    {
        using var db = DbContextFactory.Create();
        var query = WithDetails(db);
        if (statusId > 0) query = query.Where(o => o.ID_Order_Status == statusId);
        return query.OrderByDescending(o => o.Order_date).ToList().Select(ToDto).ToList();
    }

    public static List<OrderSummaryDto> GetByClient(int clientId)
    {
        using var db = DbContextFactory.Create();
        return WithDetails(db)
            .Where(o => o.ID_Client == clientId)
            .OrderByDescending(o => o.Order_date)
            .ToList()
            .Select(ToDto).ToList();
    }

    public static List<ProductOrder> GetItems(int orderId)
    {
        using var db = DbContextFactory.Create();
        var items = db.ProductOrders
            .Include(po => po.Product)
            .Where(po => po.ID_Order == orderId)
            .ToList();
        foreach (var item in items)
            item.ProductName = item.Product?.Name ?? $"[Товар #{item.ID_Product} удалён]";
        return items;
    }

    public static string? ChangeStatus(int orderId, int statusId)
    {
        using var db = DbContextFactory.Create();
        var order = db.Orders.Find(orderId);
        if (order == null) return "Заказ не найден.";
        if (!db.OrderStatuses.Any(s => s.ID_Order_Status == statusId)) return "Статус не найден.";
        order.ID_Order_Status = statusId;
        db.SaveChanges();
        return null;
    }

    public static string? ChangeDeliveryAddress(int orderId, string newAddress)
    {
        newAddress = newAddress.Trim();
        if (string.IsNullOrWhiteSpace(newAddress)) return "Адрес доставки не может быть пустым.";
        if (newAddress.Length > 500)               return "Адрес не должен превышать 500 символов.";

        using var db = DbContextFactory.Create();
        var order = db.Orders.Find(orderId);
        if (order == null) return "Заказ не найден.";

        var status = db.OrderStatuses.Find(order.ID_Order_Status);
        if (status?.Name is "Доставлен" or "Отменён")
            return $"Нельзя изменить адрес заказа со статусом «{status.Name}».";

        order.Delivery_Address = newAddress;
        db.SaveChanges();
        return null;
    }

    public static string? UpdateOrder(int orderId, int clientId, int statusId,
        string deliveryAddress, DateTime orderDate)
    {
        deliveryAddress = deliveryAddress.Trim();
        if (string.IsNullOrWhiteSpace(deliveryAddress)) return "Адрес доставки не может быть пустым.";
        if (deliveryAddress.Length > 500)               return "Адрес не должен превышать 500 символов.";

        using var db = DbContextFactory.Create();
        var order = db.Orders.Find(orderId);
        if (order == null) return "Заказ не найден.";
        if (!db.Clients.Any(c => c.ID_Client == clientId))   return "Клиент не найден.";
        if (!db.OrderStatuses.Any(s => s.ID_Order_Status == statusId)) return "Статус не найден.";

        order.ID_Client          = clientId;
        order.ID_Order_Status    = statusId;
        order.Delivery_Address   = deliveryAddress;
        order.Order_date         = orderDate;
        db.SaveChanges();
        return null;
    }

    public static string? CancelOrder(int orderId)
    {
        using var db = DbContextFactory.Create();
        var order = db.Orders.Find(orderId);
        if (order == null) return "Заказ не найден.";

        var cancelStatus = db.OrderStatuses.FirstOrDefault(s => s.Name == "Отменён");
        if (cancelStatus == null) return "Статус «Отменён» не найден. Создайте его в разделе статусов.";

        order.ID_Order_Status = cancelStatus.ID_Order_Status;
        db.SaveChanges();
        return null;
    }

    public static string? MarkAsProcessed(int orderId)
    {
        using var db = DbContextFactory.Create();
        var order = db.Orders.Find(orderId);
        if (order == null) return "Заказ не найден.";

        var processedStatus = db.OrderStatuses.FirstOrDefault(s => s.Name == "Оформлен");
        if (processedStatus == null) return "Статус «Оформлен» не найден. Создайте его в разделе статусов.";

        order.ID_Order_Status = processedStatus.ID_Order_Status;
        db.SaveChanges();
        return null;
    }

    public static string? AddItem(int orderId, int productId, int quantity)
    {
        if (quantity <= 0) return "Количество должно быть больше нуля.";

        using var db = DbContextFactory.Create();
        if (!db.Orders.Any(o => o.ID_Order == orderId))    return "Заказ не найден.";
        var product = db.Products.Find(productId);
        if (product == null) return "Товар не найден.";

        var existing = db.ProductOrders
            .FirstOrDefault(po => po.ID_Order == orderId && po.ID_Product == productId);

        if (existing != null)
            existing.Quantity += quantity;
        else
            db.ProductOrders.Add(new ProductOrder
            {
                ID_Order       = orderId,
                ID_Product     = productId,
                Quantity       = quantity,
                Price_at_order = product.Price
            });

        db.SaveChanges();
        return null;
    }

    public static string? RemoveItem(int orderId, int productId)
    {
        using var db = DbContextFactory.Create();
        var item = db.ProductOrders
            .FirstOrDefault(po => po.ID_Order == orderId && po.ID_Product == productId);
        if (item == null) return "Позиция не найдена.";
        db.ProductOrders.Remove(item);
        db.SaveChanges();
        return null;
    }

    public static string? UpdateItemQuantity(int orderId, int productId, int quantity)
    {
        if (quantity < 0) return "Количество не может быть отрицательным.";
        if (quantity == 0) return RemoveItem(orderId, productId);

        using var db = DbContextFactory.Create();
        var item = db.ProductOrders
            .FirstOrDefault(po => po.ID_Order == orderId && po.ID_Product == productId);
        if (item == null) return "Позиция не найдена.";
        item.Quantity = quantity;
        db.SaveChanges();
        return null;
    }

    public static string? Delete(int orderId)
    {
        using var db = DbContextFactory.Create();
        var order = db.Orders.Find(orderId);
        if (order == null) return "Заказ не найден.";
        db.Orders.Remove(order);
        db.SaveChanges();
        return null;
    }
}
