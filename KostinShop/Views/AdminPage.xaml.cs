using KostinShop.Helpers;
using KostinShop.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KostinShop.Views;

public partial class AdminPage : Page
{
    public AdminPage()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
        => UsersDataGrid.ItemsSource = UserService.GetAll(SearchTextBox?.Text);

    private UserService.UserListDto? Selected
        => UsersDataGrid.SelectedItem as UserService.UserListDto;

    private void SearchTextBox_TextChanged(object s, TextChangedEventArgs e) => LoadData();

    private void RefreshButton_Click(object s, RoutedEventArgs e)
    {
        SearchTextBox.Clear();
        LoadData();
    }

    private void EditButton_Click(object s, RoutedEventArgs e)
    {
        if (Selected is not { } dto)
        {
            UiHelper.ShowWarning("Выберите пользователя для редактирования.", "Ничего не выбрано");
            return;
        }
        OpenEdit(dto.ID_User);
    }

    private void UsersDataGrid_MouseDoubleClick(object s, MouseButtonEventArgs e)
    {
        if (Selected is { } dto) OpenEdit(dto.ID_User);
    }

    private void OpenEdit(int userId)
    {
        var user = UserService.GetById(userId);
        if (user == null) return;

        var dlg = new UserEditDialog(user) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) LoadData();
    }

    private void DeleteButton_Click(object s, RoutedEventArgs e)
    {
        if (Selected is not { } dto)
        {
            UiHelper.ShowWarning("Выберите пользователя для удаления.", "Ничего не выбрано");
            return;
        }

        if (!UiHelper.Confirm(
            $"Удалить пользователя «{dto.Login}» ({dto.FullName})?\n\n" +
            (dto.IsClient
                ? "Вместе с ним будет удалён клиентский профиль, корзина и все заказы."
                : "Все данные пользователя будут удалены безвозвратно."),
            "Подтверждение удаления")) return;

        if (UiHelper.TryRun(
                () => UserService.Delete(dto.ID_User, AuthService.CurrentUser!.ID_User),
                "Ошибка удаления"))
            LoadData();
    }
}
