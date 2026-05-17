using KostinShop.Helpers;
using KostinShop.Services;
using System.Windows;
using System.Windows.Input;

namespace KostinShop.Views;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
        PhoneInputHelper.Attach(PhoneTextBox);
        FirstNameTextBox.Focus();
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) RegisterButton_Click(sender, e);
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        HideError();

        var error = AuthService.Register(
            login:           LoginTextBox.Text,
            password:        PasswordBox.Password,
            confirmPassword: ConfirmPasswordBox.Password,
            firstName:       FirstNameTextBox.Text,
            middleName:      MiddleNameTextBox.Text,
            lastName:        LastNameTextBox.Text,
            phone:           PhoneTextBox.Text,
            isClient:        true,
            staffRoles:      []);

        if (error != null) { ShowError(error); return; }

        UiHelper.ShowInfo(
            "Аккаунт успешно создан!\n\n" +
            "Теперь вы можете войти в систему с указанным логином и паролем.",
            "Регистрация завершена");
        Close();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowError(string msg) { ErrorTextBlock.Text = msg; ErrorTextBlock.Visibility = Visibility.Visible; }
    private void HideError()           => ErrorTextBlock.Visibility = Visibility.Collapsed;
}
