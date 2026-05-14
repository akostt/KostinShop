using KostinShop.Data;
using KostinShop.Models;
using Microsoft.EntityFrameworkCore;

namespace KostinShop.Services;

public static class UserService
{
    public record UserListDto(
        int     ID_User,
        string  Login,
        string  FullName,
        string  Phone,
        string  Roles,
        bool    IsClient,
        int?    ID_Client
    );

    public static List<UserListDto> GetAll(string? search = null)
    {
        using var db = DbContextFactory.Create();
        var q = db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            q = q.Where(u =>
                u.Login.ToLower().Contains(s)      ||
                u.Last_Name.ToLower().Contains(s)  ||
                u.First_Name.ToLower().Contains(s) ||
                u.Phone.Contains(s));
        }

        return q.OrderBy(u => u.Last_Name).ThenBy(u => u.First_Name)
            .ToList()
            .Select(u => new UserListDto(
                u.ID_User, u.Login, u.FullName, u.Phone,
                BuildRoleString(u), u.ID_Client.HasValue, u.ID_Client))
            .ToList();
    }

    public static AppUser? GetById(int id)
    {
        using var db = DbContextFactory.Create();
        return db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.ID_User == id);
    }

    public static string? Update(
        int           userId,
        string        login,
        string?       newPassword,
        List<string>  roleNames,
        bool          wantsClient)
    {
        login = login.Trim();
        if (string.IsNullOrWhiteSpace(login))  return "Логин не может быть пустым.";
        if (login.Length < 3)                  return "Логин должен содержать минимум 3 символа.";

        if (!string.IsNullOrEmpty(newPassword))
        {
            var pwdErr = ValidationHelper.ValidatePassword(newPassword);
            if (pwdErr != null) return pwdErr;
        }

        using var db = DbContextFactory.Create();

        var user = db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefault(u => u.ID_User == userId);
        if (user == null) return "Пользователь не найден.";

        if (db.Users.Any(u => u.Login == login && u.ID_User != userId))
            return "Этот логин уже занят другим пользователем.";

        user.Login = login;
        if (!string.IsNullOrEmpty(newPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        if (wantsClient && !user.ID_Client.HasValue)
        {
            var client = new Client { Loyalty_Points = 0, Registered_At = DateTime.UtcNow };
            db.Clients.Add(client);
            db.SaveChanges();
            user.ID_Client = client.ID_Client;
        }
        else if (!wantsClient && user.ID_Client.HasValue)
        {
            user.ID_Client = null;
        }

        db.UserRoles.RemoveRange(user.UserRoles);

        foreach (var roleName in roleNames)
        {
            var role = db.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null) continue;
            db.UserRoles.Add(new UserRole { ID_User = userId, ID_Role = role.ID_Role });
        }

        db.SaveChanges();
        return null;
    }

    public static string? Delete(int userId, int currentUserId)
    {
        if (userId == currentUserId)
            return "Нельзя удалить собственную учётную запись.";

        using var db = DbContextFactory.Create();
        var user = db.Users.Find(userId);
        if (user == null) return "Пользователь не найден.";

        if (user.ID_Client.HasValue)
        {
            var client = db.Clients.Find(user.ID_Client.Value);
            if (client != null) db.Clients.Remove(client);
        }

        db.Users.Remove(user);
        db.SaveChanges();
        return null;
    }

    public static List<Role> GetAllRoles()
    {
        using var db = DbContextFactory.Create();
        return db.Roles.OrderBy(r => r.Name).ToList();
    }

    private static string BuildRoleString(AppUser u)
    {
        var parts = u.UserRoles
            .Select(ur => ur.Role?.Name)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => AuthService.RoleDisplayName(n!))
            .ToList();

        if (u.ID_Client.HasValue) parts.Insert(0, "Клиент");
        return parts.Count > 0 ? string.Join(", ", parts) : "—";
    }
}
