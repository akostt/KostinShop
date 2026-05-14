using KostinShop.Data;
using KostinShop.Models;
using Microsoft.EntityFrameworkCore;

namespace KostinShop.Services;

public static class ClientService
{
    private static IQueryable<Client> WithUser(ShopDbContext db) =>
        db.Clients.Include(c => c.User);

    private static Client MapUser(Client cl)
    {
        if (cl.User is { } u)
        {
            cl.First_Name  = u.First_Name;
            cl.Middle_Name = u.Middle_Name;
            cl.Last_Name   = u.Last_Name;
            cl.Phone       = u.Phone;
        }
        return cl;
    }

    public static List<Client> GetAll(string? search = null)
    {
        using var db = DbContextFactory.Create();

        var clients = WithUser(db)
            .Include(c => c.Orders)
            .ToList();

        foreach (var cl in clients)
        {
            MapUser(cl);
            cl.OrderCount = cl.Orders.Count;
        }

        if (string.IsNullOrWhiteSpace(search))
            return clients;

        var s = search.ToLower();
        return clients.Where(c =>
            c.Last_Name.ToLower().Contains(s)  ||
            c.First_Name.ToLower().Contains(s) ||
            c.Middle_Name?.ToLower().Contains(s) == true ||
            c.Phone.Contains(s)).ToList();
    }

    public static Client? GetById(int id)
    {
        using var db = DbContextFactory.Create();
        var cl = WithUser(db).FirstOrDefault(c => c.ID_Client == id);
        return cl is null ? null : MapUser(cl);
    }

    public static string? Update(int clientId,
        string firstName, string? middleName, string lastName,
        string phone, int loyaltyPoints)
    {
        if (string.IsNullOrWhiteSpace(firstName)) return "Имя не может быть пустым.";
        if (string.IsNullOrWhiteSpace(lastName))  return "Фамилия не может быть пустой.";

        var error = ValidationHelper.NormalizePhone(phone, out var normalizedPhone);
        if (error != null) return error;

        using var db = DbContextFactory.Create();
        var user = db.Users.FirstOrDefault(u => u.ID_Client == clientId);
        if (user == null) return "Пользователь клиента не найден.";

        if (db.Users.Any(u => u.Phone == normalizedPhone && u.ID_User != user.ID_User))
            return "Телефон уже используется другим пользователем.";

        user.First_Name  = firstName.Trim();
        user.Middle_Name = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim();
        user.Last_Name   = lastName.Trim();
        user.Phone       = normalizedPhone;

        var cl = db.Clients.Find(clientId);
        if (cl != null)
            cl.Loyalty_Points = Math.Max(0, loyaltyPoints);

        db.SaveChanges();
        return null;
    }

    public static string? Delete(int clientId)
    {
        using var db = DbContextFactory.Create();
        var cl = db.Clients.Find(clientId);
        if (cl == null) return "Клиент не найден.";

        var user = db.Users.FirstOrDefault(u => u.ID_Client == clientId);
        if (user != null) db.Users.Remove(user);

        db.Clients.Remove(cl);
        db.SaveChanges();
        return null;
    }
}
