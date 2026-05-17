using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KostinShop.Data;

public static class DbContextFactory
{
    private static string _connectionString = string.Empty;

    public static void Initialize(string connectionString) =>
        _connectionString = connectionString;

    public static ShopDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseSqlServer(_connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        return new ShopDbContext(options);
    }
}
