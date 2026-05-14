using KostinShop.Views;
using System.Windows.Controls;

namespace KostinShop.Helpers;

public static class NavigationService
{
    public static Page CreatePage(string tag) => tag switch
    {
        "Categories" => new CategoriesPage(),
        "Products"   => new ProductsPage(),
        "Clients"    => new ClientsPage(),
        "Orders"     => new OrdersPage(),
        "Statuses"   => new StatusesPage(),
        "Users"      => new AdminPage(),
        _            => new CategoriesPage()
    };
}
