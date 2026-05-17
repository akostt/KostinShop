using System.Windows;
using System.Windows.Controls;

namespace KostinShop.Views;

public abstract class DialogBase : Window
{
    protected abstract TextBlock ErrorBlock { get; }

    protected void ShowError(string msg)
    {
        ErrorBlock.Text       = msg;
        ErrorBlock.Visibility = Visibility.Visible;
    }

    protected void HideError() => ErrorBlock.Visibility = Visibility.Collapsed;

    protected void SaveAndClose(Func<string?> action)
    {
        HideError();
        var error = action();
        if (error != null) { ShowError(error); return; }
        DialogResult = true;
        Close();
    }

    protected void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
