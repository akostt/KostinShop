using KostinShop.Data;
using KostinShop.Models;
using Microsoft.EntityFrameworkCore;

namespace KostinShop.Services;

public static class AuthService
{
    public const string RoleAdmin   = "role_admin";
    public const string RoleManager = "role_manager";
    public const string RoleLogist  = "role_logist";

    public static AppUser? CurrentUser { get; private set; }

    public static bool HasRole(string roleName) =>
        CurrentUser?.RoleNames.Contains(roleName) == true;

    public static bool IsBuyer   => CurrentUser?.ID_Client != null;
    public static bool IsAdmin   => HasRole(RoleAdmin);
    public static bool IsManager => HasRole(RoleManager);
    public static bool IsLogist  => HasRole(RoleLogist);
    public static bool IsStaff   => IsAdmin || IsManager || IsLogist;

    public static string PrimaryRole =>
        IsAdmin   ? RoleAdmin   :
        IsManager ? RoleManager :
        IsLogist  ? RoleLogist  :
        "role_buyer";

    public static string RoleDisplayName(string roleName) => roleName switch
    {
        RoleAdmin   => "Администратор",
        RoleManager => "Менеджер",
        RoleLogist  => "Логист",
        _           => roleName
    };

    public static AppUser? Login(string login, string password)
    {
        using var db = DbContextFactory.Create();
        var user = db.Users
            .Include(u => u.Client)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Login == login);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        CurrentUser = user;
        return user;
    }

    public static void Logout() => CurrentUser = null;

    public static string? Register(
        string login, string password, string confirmPassword,
        string firstName, string? middleName, string lastName,
        string phone,
        bool isClient         = true,
        List<string>? staffRoles = null)
    {
        staffRoles ??= [];

        if (!isClient && staffRoles.Count == 0)
            return "Выберите хотя бы одну роль для пользователя.";

        if (string.IsNullOrWhiteSpace(login))     return "Логин не может быть пустым.";
        if (login.Trim().Length < 3)              return "Логин должен содержать минимум 3 символа.";
        if (string.IsNullOrWhiteSpace(firstName)) return "Имя не может быть пустым.";
        if (string.IsNullOrWhiteSpace(lastName))  return "Фамилия не может быть пустой.";

        var phoneError = ValidationHelper.NormalizePhone(phone, out var normalizedPhone);
        if (phoneError != null) return phoneError;

        var pwdError = ValidationHelper.ValidatePassword(password);
        if (pwdError != null) return pwdError;
        if (password != confirmPassword) return "Пароли не совпадают.";

        using var db = DbContextFactory.Create();

        if (db.Users.Any(u => u.Login == login.Trim()))
            return "Пользователь с таким логином уже существует.";
        if (db.Users.Any(u => u.Phone == normalizedPhone))
            return "Пользователь с таким номером телефона уже зарегистрирован.";

        Client? client = null;
        if (isClient)
        {
            client = new Client { Loyalty_Points = 0, Registered_At = DateTime.UtcNow };
            db.Clients.Add(client);
            db.SaveChanges();
        }

        var user = new AppUser
        {
            Login        = login.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            First_Name   = firstName.Trim(),
            Middle_Name  = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim(),
            Last_Name    = lastName.Trim(),
            Phone        = normalizedPhone,
            ID_Client    = client?.ID_Client
        };
        db.Users.Add(user);
        db.SaveChanges();

        foreach (var roleName in staffRoles)
        {
            var role = db.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null) continue;
            db.UserRoles.Add(new UserRole { ID_User = user.ID_User, ID_Role = role.ID_Role });
        }
        if (staffRoles.Count > 0)
            db.SaveChanges();

        return null;
    }
}
