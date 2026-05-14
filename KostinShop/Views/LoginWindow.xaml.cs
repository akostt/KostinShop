using KostinShop.Helpers;
using KostinShop.Services;
using System.Windows;
using System.Windows.Input;

namespace KostinShop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        LoginTextBox.Focus();
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) LoginButton_Click(sender, e);
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        HideError();

        var login    = LoginTextBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Заполните логин и пароль.");
            return;
        }

        var user = AuthService.Login(login, password);
        if (user == null)
        {
            ShowError("Неверный логин или пароль. Проверьте введённые данные.");
            PasswordBox.Clear();
            PasswordBox.Focus();
            return;
        }

        Window nextWindow;
        if (AuthService.IsAdmin || AuthService.IsStaff)
        {
            nextWindow = new MainWindow();
        }
        else if (AuthService.IsBuyer)
        {
            if (user.Client == null)
            {
                ShowError("Клиентский профиль не найден. Обратитесь к администратору.");
                AuthService.Logout();
                return;
            }
            nextWindow = new ClientWindow(user);
        }
        else
        {
            ShowError("Ни одной поддерживаемой роли не найдено. Обратитесь к администратору.");
            AuthService.Logout();
            return;
        }

        nextWindow.Show();
        Close();
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
        => new RegisterWindow { Owner = this }.ShowDialog();

    private void ShowError(string msg)
    {
        ErrorTextBlock.Text       = msg;
        ErrorTextBlock.Visibility = Visibility.Visible;
    }

    private void HideError() => ErrorTextBlock.Visibility = Visibility.Collapsed;
}
