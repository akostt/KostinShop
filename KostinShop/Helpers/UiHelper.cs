using System.Windows;

namespace KostinShop.Helpers;

public static class UiHelper
{
    public static void SetVisibility(bool isVisible, params UIElement[] controls)
    {
        var visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        foreach (var control in controls)
            control.Visibility = visibility;
    }

    public static bool Confirm(string message, string title = "Подтверждение")
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
           == MessageBoxResult.Yes;

    public static void ShowError(string message, string title = "Ошибка")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public static void ShowWarning(string message, string title = "Предупреждение")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public static void ShowInfo(string message, string title = "Информация")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public static bool TryRun(Func<string?> action, string errorTitle = "Ошибка")
    {
        var error = action();
        if (error is null) return true;
        ShowWarning(error, errorTitle);
        return false;
    }
}
