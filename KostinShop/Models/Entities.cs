using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KostinShop.Models;

public class Category
{
    [Key]
    public int ID_Category { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = [];

    [NotMapped]
    public int ProductCount { get; set; }
}

public class Product
{
    [Key]
    public int ID_Product { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public int ID_Category { get; set; }

    [ForeignKey(nameof(ID_Category))]
    public Category Category { get; set; } = null!;

    public ICollection<ProductOrder> ProductOrders { get; set; } = [];
    public ICollection<Cart> CartItems { get; set; } = [];

    [NotMapped]
    public string CategoryName { get; set; } = string.Empty;
}

public class Client
{
    [Key]
    public int ID_Client { get; set; }

    [Required]
    public int Loyalty_Points { get; set; } = 0;

    public DateTime Registered_At { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Cart> CartItems { get; set; } = [];

    [NotMapped]
    public int OrderCount { get; set; }

    public AppUser? User { get; set; }

    [NotMapped]
    public string First_Name { get; set; } = string.Empty;
    [NotMapped]
    public string? Middle_Name { get; set; }
    [NotMapped]
    public string Last_Name { get; set; } = string.Empty;
    [NotMapped]
    public string Phone { get; set; } = string.Empty;
    [NotMapped]
    public string FullName => $"{Last_Name} {First_Name}".Trim();
}

public class OrderStatus
{
    [Key]
    public int ID_Order_Status { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = [];

    [NotMapped]
    public int OrderCount { get; set; }
}

[Table("Order")]
public class Order
{
    [Key]
    public int ID_Order { get; set; }

    public int ID_Client { get; set; }

    [ForeignKey(nameof(ID_Client))]
    public Client Client { get; set; } = null!;

    [Required, MaxLength(500)]
    public string Delivery_Address { get; set; } = string.Empty;

    public DateTime Order_date { get; set; } = DateTime.Now;

    public int ID_Order_Status { get; set; }

    [ForeignKey(nameof(ID_Order_Status))]
    public OrderStatus OrderStatus { get; set; } = null!;

    public ICollection<ProductOrder> ProductOrders { get; set; } = [];
}

public class ProductOrder
{
    [Key]
    public int ID_Product_Order { get; set; }

    public int ID_Order { get; set; }

    [ForeignKey(nameof(ID_Order))]
    public Order Order { get; set; } = null!;

    public int ID_Product { get; set; }

    [ForeignKey(nameof(ID_Product))]
    public Product Product { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price_at_order { get; set; }

    [NotMapped]
    public string ProductName { get; set; } = string.Empty;

    [NotMapped]
    public decimal LineTotal => Quantity * Price_at_order;
}

public class Cart
{
    [Key]
    public int ID_Cart { get; set; }

    public int ID_Client { get; set; }

    [ForeignKey(nameof(ID_Client))]
    public Client Client { get; set; } = null!;

    public int ID_Product { get; set; }

    [ForeignKey(nameof(ID_Product))]
    public Product Product { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }

    [NotMapped]
    public string ProductName { get; set; } = string.Empty;

    [NotMapped]
    public decimal Price { get; set; }

    [NotMapped]
    public decimal Total => Quantity * Price;
}

public class AppUser
{
    [Key]
    public int ID_User { get; set; }

    [Required, MaxLength(50)]
    public string Login { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string First_Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Middle_Name { get; set; }

    [Required, MaxLength(50)]
    public string Last_Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = [];

    public int? ID_Client { get; set; }

    [ForeignKey(nameof(ID_Client))]
    public Client? Client { get; set; }

    [NotMapped]
    public string FullName => $"{Last_Name} {First_Name}".Trim();

    [NotMapped]
    public Role? Role => UserRoles.FirstOrDefault()?.Role;

    [NotMapped]
    public IEnumerable<string> RoleNames => UserRoles.Select(ur => ur.Role?.Name ?? string.Empty);
}

public class Role
{
    [Key]
    public int ID_Role { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}

public class UserRole
{
    [Key]
    public int ID_User_Role { get; set; }

    public int ID_User { get; set; }

    [ForeignKey(nameof(ID_User))]
    public AppUser User { get; set; } = null!;

    public int ID_Role { get; set; }

    [ForeignKey(nameof(ID_Role))]
    public Role Role { get; set; } = null!;
}
