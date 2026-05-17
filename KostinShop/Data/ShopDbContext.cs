using KostinShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KostinShop.Data;

public class ShopDbContext : DbContext
{
    public DbSet<Category>    Categories    { get; set; }
    public DbSet<Product>     Products      { get; set; }
    public DbSet<Client>      Clients       { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }
    public DbSet<Order>       Orders        { get; set; }
    public DbSet<ProductOrder> ProductOrders { get; set; }
    public DbSet<Cart>        Carts         { get; set; }
    public DbSet<AppUser>     Users         { get; set; }
    public DbSet<Role>        Roles         { get; set; }
    public DbSet<UserRole>    UserRoles     { get; set; }

    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().ToTable("Category");
        modelBuilder.Entity<Client>().ToTable("Client");
        modelBuilder.Entity<OrderStatus>().ToTable("Order_Status");
        modelBuilder.Entity<Order>().ToTable("Order");
        modelBuilder.Entity<Product>().ToTable("Product");
        modelBuilder.Entity<ProductOrder>().ToTable("Product_Order");
        modelBuilder.Entity<Cart>().ToTable("Cart");
        modelBuilder.Entity<AppUser>().ToTable("AppUser");
        modelBuilder.Entity<Role>().ToTable("Role");
        modelBuilder.Entity<UserRole>().ToTable("UserRole");

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Login).IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Phone).IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.ID_Client).IsUnique()
            .HasFilter("[ID_Client] IS NOT NULL");  // NULL = нет клиентского профиля (сотрудник)

        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => new { ur.ID_User, ur.ID_Role }).IsUnique();

        modelBuilder.Entity<Cart>()
            .HasIndex(c => new { c.ID_Client, c.ID_Product }).IsUnique();

        modelBuilder.Entity<ProductOrder>()
            .HasIndex(po => new { po.ID_Order, po.ID_Product }).IsUnique();

        modelBuilder.Entity<Product>()
            .ToTable(t => t.HasCheckConstraint("CK_Product_Price", "[Price] >= 0"));
        modelBuilder.Entity<ProductOrder>()
            .ToTable(t => t.HasCheckConstraint("CK_ProductOrder_Qty", "[Quantity] > 0"));
        modelBuilder.Entity<Cart>()
            .ToTable(t => t.HasCheckConstraint("CK_Cart_Qty", "[Quantity] > 0"));
        modelBuilder.Entity<Client>()
            .ToTable(t => t.HasCheckConstraint("CK_Client_LoyaltyPts", "[Loyalty_Points] >= 0"));
    }
}
