using KostinShop.Data;
using KostinShop.Models;
using Microsoft.EntityFrameworkCore;

namespace KostinShop.Services;

public static class StatusService
{
    public static List<OrderStatus> GetAll(string? search = null)
    {
        using var db = DbContextFactory.Create();
        var query = db.OrderStatuses.Include(s => s.Orders).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.Contains(search));

        var result = query.OrderBy(s => s.Name).ToList();
        foreach (var s in result)
            s.OrderCount = s.Orders.Count;
        return result;
    }

    public static List<OrderStatus> GetAllSimple()
    {
        using var db = DbContextFactory.Create();
        return db.OrderStatuses.OrderBy(s => s.Name).ToList();
    }

    public static string? Add(string name)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return "Название не может быть пустым.";
        if (name.Length > 50)               return "Название не должно превышать 50 символов.";

        using var db = DbContextFactory.Create();
        if (db.OrderStatuses.Any(s => s.Name == name))
            return "Статус с таким названием уже существует.";

        db.OrderStatuses.Add(new OrderStatus { Name = name });
        db.SaveChanges();
        return null;
    }

    public static string? Update(int id, string name)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return "Название не может быть пустым.";
        if (name.Length > 50)               return "Название не должно превышать 50 символов.";

        using var db = DbContextFactory.Create();
        if (db.OrderStatuses.Any(s => s.Name == name && s.ID_Order_Status != id))
            return "Статус с таким названием уже существует.";

        var status = db.OrderStatuses.Find(id);
        if (status == null) return "Статус не найден.";

        status.Name = name;
        db.SaveChanges();
        return null;
    }

    public static string? Delete(int id)
    {
        using var db = DbContextFactory.Create();
        var status = db.OrderStatuses.Include(s => s.Orders).FirstOrDefault(s => s.ID_Order_Status == id);
        if (status == null) return "Статус не найден.";
        if (status.Orders.Count > 0)
            return $"Невозможно удалить статус: он используется в {status.Orders.Count} заказах.";

        db.OrderStatuses.Remove(status);
        db.SaveChanges();
        return null;
    }
}
