using KostinShop.Models;
using KostinShop.Services;
using System.Windows;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class UserEditDialog : DialogBase
{
    private readonly AppUser _user;

    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public UserEditDialog(AppUser user)
    {
        InitializeComponent();
        _user = user;

        FullNameTextBlock.Text = $"{user.Last_Name} {user.First_Name} {user.Middle_Name}".Trim()
                               + $"  ·  {user.Phone}";
        LoginTextBox.Text = user.Login;

        var roleNames = user.UserRoles.Select(ur => ur.Role?.Name ?? "").ToHashSet();
        RoleClientCheck.IsChecked  = user.ID_Client.HasValue;
        RoleAdminCheck.IsChecked   = roleNames.Contains(AuthService.RoleAdmin);
        RoleManagerCheck.IsChecked = roleNames.Contains(AuthService.RoleManager);
        RoleLogistCheck.IsChecked  = roleNames.Contains(AuthService.RoleLogist);

        RoleClientCheck.Unchecked += (_, _) =>
        {
            if (user.ID_Client.HasValue)
                ClientWarningText.Visibility = Visibility.Visible;
        };
        RoleClientCheck.Checked += (_, _) =>
            ClientWarningText.Visibility = Visibility.Collapsed;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var roles = new List<string>();
        if (RoleAdminCheck.IsChecked   == true) roles.Add(AuthService.RoleAdmin);
        if (RoleManagerCheck.IsChecked == true) roles.Add(AuthService.RoleManager);
        if (RoleLogistCheck.IsChecked  == true) roles.Add(AuthService.RoleLogist);

        var wantsClient = RoleClientCheck.IsChecked == true;
        var pwd = NewPasswordBox.Password;
        SaveAndClose(() => UserService.Update(
            _user.ID_User,
            LoginTextBox.Text,
            string.IsNullOrWhiteSpace(pwd) ? null : pwd,
            roles,
            wantsClient));
    }
}
