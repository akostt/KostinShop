using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KostinShop.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    ID_Category = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.ID_Category);
                });

            migrationBuilder.CreateTable(
                name: "Order_Status",
                columns: table => new
                {
                    ID_Order_Status = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Status", x => x.ID_Order_Status);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    ID_Role = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.ID_Role);
                });

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    ID_Client     = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Loyalty_Points = table.Column<int>(nullable: false, defaultValue: 0),
                    Registered_At  = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.ID_Client);
                    table.CheckConstraint("CK_Client_LoyaltyPts", "[Loyalty_Points] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    ID_Product  = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name        = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    Price       = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ID_Category = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.ID_Product);
                    table.CheckConstraint("CK_Product_Price", "[Price] >= 0");
                    table.ForeignKey("FK_Product_Category", x => x.ID_Category,
                        "Category", "ID_Category", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppUser",
                columns: table => new
                {
                    ID_User      = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Login        = table.Column<string>(maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(maxLength: 256, nullable: false),
                    First_Name   = table.Column<string>(maxLength: 50, nullable: false),
                    Middle_Name  = table.Column<string>(maxLength: 50, nullable: true),
                    Last_Name    = table.Column<string>(maxLength: 50, nullable: false),
                    Phone        = table.Column<string>(maxLength: 20, nullable: false),
                    ID_Client    = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUser", x => x.ID_User);
                    table.ForeignKey("FK_AppUser_Client", x => x.ID_Client,
                        "Client", "ID_Client", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    ID_User_Role = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_User = table.Column<int>(nullable: false),
                    ID_Role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.ID_User_Role);
                    table.ForeignKey("FK_UserRole_User", x => x.ID_User,
                        "AppUser", "ID_User", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_UserRole_Role", x => x.ID_Role,
                        "Role", "ID_Role", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    ID_Order          = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_Client         = table.Column<int>(nullable: false),
                    Delivery_Address  = table.Column<string>(maxLength: 500, nullable: false),
                    Order_date        = table.Column<DateTime>(nullable: false),
                    ID_Order_Status   = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.ID_Order);
                    table.ForeignKey("FK_Order_Client", x => x.ID_Client,
                        "Client", "ID_Client", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_Order_Status", x => x.ID_Order_Status,
                        "Order_Status", "ID_Order_Status", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cart",
                columns: table => new
                {
                    ID_Cart    = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_Client  = table.Column<int>(nullable: false),
                    ID_Product = table.Column<int>(nullable: false),
                    Quantity   = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cart", x => x.ID_Cart);
                    table.CheckConstraint("CK_Cart_Qty", "[Quantity] > 0");
                    table.ForeignKey("FK_Cart_Client", x => x.ID_Client,
                        "Client", "ID_Client", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_Cart_Product", x => x.ID_Product,
                        "Product", "ID_Product", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Product_Order",
                columns: table => new
                {
                    ID_Product_Order = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_Order         = table.Column<int>(nullable: false),
                    ID_Product       = table.Column<int>(nullable: false),
                    Quantity         = table.Column<int>(nullable: false),
                    Price_at_order   = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product_Order", x => x.ID_Product_Order);
                    table.CheckConstraint("CK_ProductOrder_Qty", "[Quantity] > 0");
                    table.ForeignKey("FK_ProductOrder_Order", x => x.ID_Order,
                        "Order", "ID_Order", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ProductOrder_Product", x => x.ID_Product,
                        "Product", "ID_Product", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_AppUser_Login",    "AppUser", "Login",    unique: true);
            migrationBuilder.CreateIndex("IX_AppUser_Phone",    "AppUser", "Phone",    unique: true);
            migrationBuilder.CreateIndex("IX_AppUser_ID_Client","AppUser", "ID_Client",unique: true);
            migrationBuilder.CreateIndex("IX_UserRole_Pair",    "UserRole",
                new[] { "ID_User", "ID_Role" }, unique: true);
            migrationBuilder.CreateIndex("IX_Cart_Pair",        "Cart",
                new[] { "ID_Client", "ID_Product" }, unique: true);
            migrationBuilder.CreateIndex("IX_ProductOrder_Pair","Product_Order",
                new[] { "ID_Order", "ID_Product" }, unique: true);
            migrationBuilder.CreateIndex("IX_Product_Category", "Product",     "ID_Category");
            migrationBuilder.CreateIndex("IX_Order_Client",     "Order",       "ID_Client");
            migrationBuilder.CreateIndex("IX_Order_Status",     "Order",       "ID_Order_Status");
            migrationBuilder.CreateIndex("IX_Cart_Product",     "Cart",        "ID_Product");
            migrationBuilder.CreateIndex("IX_ProductOrder_Prod","Product_Order","ID_Product");
            migrationBuilder.CreateIndex("IX_UserRole_User",    "UserRole",    "ID_User");
            migrationBuilder.CreateIndex("IX_UserRole_Role",    "UserRole",    "ID_Role");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Product_Order");
            migrationBuilder.DropTable("Cart");
            migrationBuilder.DropTable("Order");
            migrationBuilder.DropTable("UserRole");
            migrationBuilder.DropTable("AppUser");
            migrationBuilder.DropTable("Product");
            migrationBuilder.DropTable("Client");
            migrationBuilder.DropTable("Role");
            migrationBuilder.DropTable("Order_Status");
            migrationBuilder.DropTable("Category");
        }
    }
}
