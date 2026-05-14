using KostinShop.Data;
using KostinShop.Models;
using Microsoft.EntityFrameworkCore;

namespace KostinShop.Services;

public static class CartService
{
    public static List<Cart> GetCart(int clientId)
    {
        using var db = DbContextFactory.Create();
        var items = db.Carts
            .Include(c => c.Product)
            .Where(c => c.ID_Client == clientId)
            .ToList();

        foreach (var item in items)
        {
            item.ProductName = item.Product?.Name ?? $"[Товар #{item.ID_Product} удалён]";
            item.Price       = item.Product?.Price ?? 0m;
        }
        return items;
    }

    public static string? AddToCart(int clientId, int productId, int quantity = 1)
    {
        if (quantity <= 0) return "Количество должно быть больше нуля.";

        using var db = DbContextFactory.Create();
        var existing = db.Carts.FirstOrDefault(c => c.ID_Client == clientId && c.ID_Product == productId);

        if (existing != null)
            existing.Quantity += quantity;
        else
            db.Carts.Add(new Cart { ID_Client = clientId, ID_Product = productId, Quantity = quantity });

        db.SaveChanges();
        return null;
    }

    public static string? UpdateQuantity(int cartId, int quantity)
    {
        using var db = DbContextFactory.Create();
        var item = db.Carts.Find(cartId);
        if (item == null) return "Позиция корзины не найдена.";

        if (quantity <= 0) db.Carts.Remove(item);
        else               item.Quantity = quantity;

        db.SaveChanges();
        return null;
    }

    public static string? Remove(int cartId)
    {
        using var db = DbContextFactory.Create();
        var item = db.Carts.Find(cartId);
        if (item == null) return "Позиция не найдена.";
        db.Carts.Remove(item);
        db.SaveChanges();
        return null;
    }

    public static string? PlaceOrder(int clientId, string deliveryAddress, int pointsToSpend, out int actualSpend)
    {
        actualSpend = 0;

        deliveryAddress = deliveryAddress.Trim();
        if (string.IsNullOrWhiteSpace(deliveryAddress)) return "Укажите адрес доставки.";
        if (deliveryAddress.Length > 500) return "Адрес доставки не должен превышать 500 символов.";
        if (pointsToSpend < 0) return "Количество списываемых баллов не может быть отрицательным.";

        using var db = DbContextFactory.Create();

        var client = db.Clients.Find(clientId);
        if (client == null) return "Клиент не найден.";

        var cartItems = db.Carts
            .Include(c => c.Product)
            .Where(c => c.ID_Client == clientId)
            .ToList();

        if (cartItems.Count == 0) return "Корзина пуста.";

        var subtotal = cartItems.Sum(i => i.Quantity * i.Product!.Price);
        var spend = Math.Min(pointsToSpend, Math.Min(client.Loyalty_Points, (int)subtotal));
        actualSpend = spend;

        var defaultStatus = db.OrderStatuses.OrderBy(s => s.ID_Order_Status).FirstOrDefault()
            ?? throw new InvalidOperationException("Нет ни одного статуса заказа.");

        var order = new Order
        {
            ID_Client = clientId,
            Delivery_Address = deliveryAddress,
            Order_date = DateTime.Now,
            ID_Order_Status = defaultStatus.ID_Order_Status,
            ProductOrders = cartItems.Select(i => new ProductOrder
            {
                ID_Product = i.ID_Product,
                Quantity = i.Quantity,
                Price_at_order = i.Product!.Price
            }).ToList()
        };
        db.Orders.Add(order);
        db.Carts.RemoveRange(cartItems);

        client.Loyalty_Points = client.Loyalty_Points - spend + (int)((subtotal - spend) / 100);

        db.SaveChanges();
        return null;
    }

    public static int GetLoyaltyPoints(int clientId)
    {
        using var db = DbContextFactory.Create();
        return db.Clients.Find(clientId)?.Loyalty_Points ?? 0;
    }
}
