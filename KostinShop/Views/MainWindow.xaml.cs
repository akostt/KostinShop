using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using KostinShop.Helpers;
using KostinShop.Services;

namespace KostinShop.Views;

public partial class MainWindow : Window
{
    private bool _sidebarExpanded = false;

    public MainWindow()
    {
        InitializeComponent();
        SetLabelsVisibility(Visibility.Collapsed);

        var user = AuthService.CurrentUser!;

        var roleLabel = BuildRoleLabel(user);
        CurrentUserTextBlock.Text = $"👤 {user.Last_Name} {user.First_Name} ({roleLabel})";

        ApplyRoleRestrictions();

        if (AuthService.IsAdmin)
            NavUsers.Visibility = Visibility.Visible;

        if (AuthService.IsBuyer)
            NavMyOrders.Visibility = Visibility.Visible;

        var startTag = AuthService.IsAdmin ? "Users"
                     : AuthService.IsManager ? "Orders"
                     : AuthService.IsLogist ? "Orders"
                     : "Categories";
        var startBtn = startTag switch { "Users" => NavUsers, "Orders" => NavOrders, _ => NavCategories };
        SetActiveNav(startBtn);
        NavigateTo(startTag);
    }

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        _sidebarExpanded = !_sidebarExpanded;
        var sb = (Storyboard)Resources[_sidebarExpanded ? "ExpandSidebar" : "CollapseSidebar"];
        sb.Begin(this);
        SetLabelsVisibility(_sidebarExpanded ? Visibility.Visible : Visibility.Collapsed);
    }

    private void SetLabelsVisibility(Visibility v)
    {
        LblCategories.Visibility = v;
        LblProducts.Visibility   = v;
        LblClients.Visibility    = v;
        LblOrders.Visibility     = v;
        LblStatuses.Visibility   = v;
        LblUsers.Visibility      = v;
        LblMyOrders.Visibility   = v;
    }

    private Button? _activeNavButton;

    private void SetActiveNav(Button btn)
    {
        if (_activeNavButton != null)
            _activeNavButton.Background = System.Windows.Media.Brushes.Transparent;

        _activeNavButton = btn;
        btn.Background = (System.Windows.Media.Brush)FindResource("Additional1Brush");
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string tag } btn) return;

        if (tag == "MyOrders")
        {
            OpenClientCabinet();
            return;
        }
        SetActiveNav(btn);
        NavigateTo(tag);
    }

    private void OpenClientCabinet()
    {
        var user = AuthService.CurrentUser!;
        if (user.ID_Client == null)
        {
            UiHelper.ShowWarning(
                "Ваш аккаунт не связан с клиентским профилем.\nОбратитесь к администратору.",
                "Нет клиентского профиля");
            return;
        }
        new ClientWindow(user) { Owner = this }.Show();
    }

    private void NavigateTo(string tag)
        => ContentFrame.Navigate(NavigationService.CreatePage(tag));

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (!UiHelper.Confirm("Вы уверены, что хотите выйти из системы?", "Подтверждение выхода")) return;
        AuthService.Logout();
        new LoginWindow().Show();
        Close();
    }

    private static string BuildRoleLabel(Models.AppUser user)
    {
        var names = user.RoleNames
            .Select(AuthService.RoleDisplayName)
            .ToList();
        if (AuthService.IsBuyer) names.Insert(0, "Покупатель");
        return string.Join(" + ", names.Distinct());
    }

    private void ApplyRoleRestrictions()
    {
        if (!AuthService.IsLogist || AuthService.IsManager) return;
        NavCategories.Visibility = Visibility.Collapsed;
        NavProducts.Visibility   = Visibility.Collapsed;
        NavClients.Visibility    = Visibility.Collapsed;
        NavStatuses.Visibility   = Visibility.Collapsed;
        NavUsers.Visibility      = Visibility.Collapsed;
    }
}
