using KostinShop.Data;
using KostinShop.Models;
using Microsoft.EntityFrameworkCore;

namespace KostinShop.Services;

public static class CategoryService
{
    public static List<Category> GetAll(string? search = null)
    {
        using var db = DbContextFactory.Create();
        var query = db.Categories.Include(c => c.Products).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search));

        var result = query.OrderBy(c => c.Name).ToList();
        foreach (var cat in result)
            cat.ProductCount = cat.Products.Count;
        return result;
    }

    public static List<Category> GetAllSimple()
    {
        using var db = DbContextFactory.Create();
        return db.Categories.OrderBy(c => c.Name).ToList();
    }

    public static string? Add(string name)
    {
        var error = ValidateName(name, out name);
        if (error != null) return error;

        using var db = DbContextFactory.Create();
        if (db.Categories.Any(c => c.Name == name))
            return "Категория с таким названием уже существует.";

        db.Categories.Add(new Category { Name = name });
        db.SaveChanges();
        return null;
    }

    public static string? Update(int id, string name)
    {
        var error = ValidateName(name, out name);
        if (error != null) return error;

        using var db = DbContextFactory.Create();
        if (db.Categories.Any(c => c.Name == name && c.ID_Category != id))
            return "Категория с таким названием уже существует.";

        var cat = db.Categories.Find(id);
        if (cat == null) return "Категория не найдена.";

        cat.Name = name;
        db.SaveChanges();
        return null;
    }

    public static string? Delete(int id)
    {
        using var db = DbContextFactory.Create();
        var cat = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.ID_Category == id);
        if (cat == null) return "Категория не найдена.";
        if (cat.Products.Count > 0)
            return $"Невозможно удалить категорию: к ней привязано {cat.Products.Count} товаров. " +
                   "Сначала удалите или переместите товары.";

        db.Categories.Remove(cat);
        db.SaveChanges();
        return null;
    }

    private static string? ValidateName(string raw, out string trimmed)
    {
        trimmed = raw.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return "Название категории не может быть пустым.";
        if (trimmed.Length > 100)               return "Название не должно превышать 100 символов.";
        return null;
    }
}
