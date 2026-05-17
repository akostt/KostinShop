using System;
using KostinShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace KostinShop.Migrations
{
    [DbContext(typeof(ShopDbContext))]
    partial class ShopDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);
            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("KostinShop.Models.AppUser", b =>
            {
                b.Property<int>("ID_User").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_User"));
                b.Property<int?>("ID_Client").HasColumnType("int");
                b.Property<string>("First_Name").IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<string>("Middle_Name").HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<string>("Last_Name").IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<string>("Phone").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<string>("Login").IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<string>("PasswordHash").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.HasKey("ID_User");
                b.HasIndex("Login").IsUnique();
                b.HasIndex("Phone").IsUnique();
                b.HasIndex("ID_Client").IsUnique().HasFilter("[ID_Client] IS NOT NULL");
                b.ToTable("AppUser");
            });

            modelBuilder.Entity("KostinShop.Models.Cart", b =>
            {
                b.Property<int>("ID_Cart").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_Cart"));
                b.Property<int>("ID_Client").HasColumnType("int");
                b.Property<int>("ID_Product").HasColumnType("int");
                b.Property<int>("Quantity").HasColumnType("int");
                b.HasKey("ID_Cart");
                b.HasIndex(new[] { "ID_Client", "ID_Product" }).IsUnique();
                b.HasIndex("ID_Product");
                b.ToTable("Cart", t => t.HasCheckConstraint("CK_Cart_Qty", "[Quantity] > 0"));
            });

            modelBuilder.Entity("KostinShop.Models.Category", b =>
            {
                b.Property<int>("ID_Category").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_Category"));
                b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.HasKey("ID_Category");
                b.ToTable("Category");
            });

            modelBuilder.Entity("KostinShop.Models.Client", b =>
            {
                b.Property<int>("ID_Client").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_Client"));
                b.Property<int>("Loyalty_Points").HasColumnType("int").HasDefaultValue(0);
                b.Property<DateTime>("Registered_At").HasColumnType("datetime2");
                b.HasKey("ID_Client");
                b.ToTable("Client", t => t.HasCheckConstraint("CK_Client_LoyaltyPts", "[Loyalty_Points] >= 0"));
            });

            modelBuilder.Entity("KostinShop.Models.Order", b =>
            {
                b.Property<int>("ID_Order").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_Order"));
                b.Property<int>("ID_Client").HasColumnType("int");
                b.Property<string>("Delivery_Address").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.Property<DateTime>("Order_date").HasColumnType("datetime2");
                b.Property<int>("ID_Order_Status").HasColumnType("int");
                b.HasKey("ID_Order");
                b.HasIndex("ID_Client");
                b.HasIndex("ID_Order_Status");
                b.ToTable("Order");
            });

            modelBuilder.Entity("KostinShop.Models.OrderStatus", b =>
            {
                b.Property<int>("ID_Order_Status").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_Order_Status"));
                b.Property<string>("Name").IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.HasKey("ID_Order_Status");
                b.ToTable("Order_Status");
            });

            modelBuilder.Entity("KostinShop.Models.Product", b =>
            {
                b.Property<int>("ID_Product").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_Product"));
                b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<string>("Description").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
                b.Property<decimal>("Price").HasColumnType("decimal(10,2)");
                b.Property<int>("ID_Category").HasColumnType("int");
                b.HasKey("ID_Product");
                b.HasIndex("ID_Category");
                b.ToTable("Product", t => t.HasCheckConstraint("CK_Product_Price", "[Price] >= 0"));
            });

            modelBuilder.Entity("KostinShop.Models.ProductOrder", b =>
            {
                b.Property<int>("ID_Product_Order").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_Product_Order"));
                b.Property<int>("ID_Order").HasColumnType("int");
                b.Property<int>("ID_Product").HasColumnType("int");
                b.Property<int>("Quantity").HasColumnType("int");
                b.Property<decimal>("Price_at_order").HasColumnType("decimal(10,2)");
                b.HasKey("ID_Product_Order");
                b.HasIndex(new[] { "ID_Order", "ID_Product" }).IsUnique();
                b.HasIndex("ID_Product");
                b.ToTable("Product_Order", t => t.HasCheckConstraint("CK_ProductOrder_Qty", "[Quantity] > 0"));
            });

            modelBuilder.Entity("KostinShop.Models.Role", b =>
            {
                b.Property<int>("ID_Role").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_Role"));
                b.Property<string>("Name").IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.Property<string>("Description").HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.HasKey("ID_Role");
                b.ToTable("Role");
            });

            modelBuilder.Entity("KostinShop.Models.UserRole", b =>
            {
                b.Property<int>("ID_User_Role").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID_User_Role"));
                b.Property<int>("ID_User").HasColumnType("int");
                b.Property<int>("ID_Role").HasColumnType("int");
                b.HasKey("ID_User_Role");
                b.HasIndex(new[] { "ID_User", "ID_Role" }).IsUnique();
                b.HasIndex("ID_Role");
                b.ToTable("UserRole");
            });


            modelBuilder.Entity("KostinShop.Models.AppUser", b =>
            {
                b.HasOne("KostinShop.Models.Client", "Client")
                    .WithOne("User")
                    .HasForeignKey("KostinShop.Models.AppUser", "ID_Client")
                    .OnDelete(DeleteBehavior.SetNull);
                b.Navigation("Client");
            });

            modelBuilder.Entity("KostinShop.Models.Cart", b =>
            {
                b.HasOne("KostinShop.Models.Client", "Client")
                    .WithMany("CartItems")
                    .HasForeignKey("ID_Client")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.HasOne("KostinShop.Models.Product", "Product")
                    .WithMany("CartItems")
                    .HasForeignKey("ID_Product")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("Client");
                b.Navigation("Product");
            });

            modelBuilder.Entity("KostinShop.Models.Order", b =>
            {
                b.HasOne("KostinShop.Models.Client", "Client")
                    .WithMany("Orders")
                    .HasForeignKey("ID_Client")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.HasOne("KostinShop.Models.OrderStatus", "OrderStatus")
                    .WithMany("Orders")
                    .HasForeignKey("ID_Order_Status")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("Client");
                b.Navigation("OrderStatus");
            });

            modelBuilder.Entity("KostinShop.Models.Product", b =>
            {
                b.HasOne("KostinShop.Models.Category", "Category")
                    .WithMany("Products")
                    .HasForeignKey("ID_Category")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("Category");
            });

            modelBuilder.Entity("KostinShop.Models.ProductOrder", b =>
            {
                b.HasOne("KostinShop.Models.Order", "Order")
                    .WithMany("ProductOrders")
                    .HasForeignKey("ID_Order")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.HasOne("KostinShop.Models.Product", "Product")
                    .WithMany("ProductOrders")
                    .HasForeignKey("ID_Product")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("Order");
                b.Navigation("Product");
            });

            modelBuilder.Entity("KostinShop.Models.UserRole", b =>
            {
                b.HasOne("KostinShop.Models.AppUser", "User")
                    .WithMany("UserRoles")
                    .HasForeignKey("ID_User")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.HasOne("KostinShop.Models.Role", "Role")
                    .WithMany("UserRoles")
                    .HasForeignKey("ID_Role")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("User");
                b.Navigation("Role");
            });

            modelBuilder.Entity("KostinShop.Models.Client", b =>
            {
                b.Navigation("Orders");
                b.Navigation("CartItems");
                b.Navigation("User");
            });

            modelBuilder.Entity("KostinShop.Models.Category", b =>
            {
                b.Navigation("Products");
            });

            modelBuilder.Entity("KostinShop.Models.Order", b =>
            {
                b.Navigation("ProductOrders");
            });

            modelBuilder.Entity("KostinShop.Models.OrderStatus", b =>
            {
                b.Navigation("Orders");
            });

            modelBuilder.Entity("KostinShop.Models.Product", b =>
            {
                b.Navigation("ProductOrders");
                b.Navigation("CartItems");
            });

            modelBuilder.Entity("KostinShop.Models.AppUser", b =>
            {
                b.Navigation("UserRoles");
            });

            modelBuilder.Entity("KostinShop.Models.Role", b =>
            {
                b.Navigation("UserRoles");
            });
#pragma warning restore 612, 618
        }
    }
}
