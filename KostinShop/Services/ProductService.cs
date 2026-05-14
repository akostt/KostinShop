using KostinShop.Data;
using KostinShop.Models;
using Microsoft.EntityFrameworkCore;

namespace KostinShop.Services;

public static class ProductService
{
    public static List<Product> GetAll(string? search = null, int? categoryId = null)
    {
        using var db = DbContextFactory.Create();
        var query = db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) ||
                                     (p.Description != null && p.Description.Contains(search)));

        if (categoryId > 0)
            query = query.Where(p => p.ID_Category == categoryId);

        var result = query.OrderBy(p => p.Name).ToList();
        foreach (var p in result)
            p.CategoryName = p.Category.Name;
        return result;
    }

    public static string? Add(string name, int categoryId, decimal price, string? description)
    {
        var error = Validate(name, categoryId, price);
        if (error != null) return error;

        using var db = DbContextFactory.Create();
        db.Products.Add(new Product
        {
            Name        = name.Trim(),
            ID_Category = categoryId,
            Price       = price,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        });
        db.SaveChanges();
        return null;
    }

    public static string? Update(int id, string name, int categoryId, decimal price, string? description)
    {
        var error = Validate(name, categoryId, price);
        if (error != null) return error;

        using var db = DbContextFactory.Create();
        var product = db.Products.Find(id);
        if (product == null) return "Товар не найден.";

        product.Name        = name.Trim();
        product.ID_Category = categoryId;
        product.Price       = price;
        product.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        db.SaveChanges();
        return null;
    }

    public static string? Delete(int id)
    {
        using var db = DbContextFactory.Create();
        var product = db.Products.Find(id);
        if (product == null) return "Товар не найден.";
        db.Products.Remove(product);
        db.SaveChanges();
        return null;
    }

    private static string? Validate(string name, int categoryId, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Название товара не может быть пустым.";
        if (name.Length > 200)               return "Название не должно превышать 200 символов.";
        if (categoryId <= 0)                 return "Выберите категорию.";
        if (price < 0)                       return "Цена не может быть отрицательной.";
        return null;
    }
}
