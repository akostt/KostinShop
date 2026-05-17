using KostinShop.Helpers;
using KostinShop.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KostinShop.Views;

public abstract class CrudPage<T> : Page where T : class
{
    protected abstract DataGrid Grid       { get; }
    protected abstract string   EntityName { get; }

    protected abstract void         LoadData();
    protected abstract void         OpenAddDialog();
    protected abstract void         OpenEditDialog(T item);
    protected abstract string       DeleteConfirmMessage(T item);
    protected abstract Func<string?> DeleteAction(T item);

    protected void ApplyAdminReadOnly(params UIElement[] controls)
    {
        if (!AuthService.IsAdmin) return;

        UiHelper.SetVisibility(false, controls);
        Grid.IsReadOnly = true;
    }

    protected void OnAdd(object sender, RoutedEventArgs e)
    {
        if (AuthService.IsAdmin) return;
        OpenAddDialog();
        LoadData();
    }

    protected void OnEdit(object sender, RoutedEventArgs e)
    {
        if (AuthService.IsAdmin) return;
        if (Grid.SelectedItem is not T item)
        {
            UiHelper.ShowWarning($"Выберите {EntityName} для редактирования.", "Ничего не выбрано");
            return;
        }
        OpenEditDialog(item);
    }

    protected void OnDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (AuthService.IsAdmin) return;
        if (Grid.SelectedItem is T item) OpenEditDialog(item);
    }

    protected void OnDelete(object sender, RoutedEventArgs e)
    {
        if (AuthService.IsAdmin) return;
        if (Grid.SelectedItem is not T item)
        {
            UiHelper.ShowWarning($"Выберите {EntityName} для удаления.", "Ничего не выбрано");
            return;
        }
        if (!UiHelper.Confirm(DeleteConfirmMessage(item), "Подтверждение удаления")) return;
        if (UiHelper.TryRun(DeleteAction(item), "Ошибка удаления")) LoadData();
    }

    protected void OpenDialogAndRefresh(Window dialog)
    {
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() == true) LoadData();
    }
}

public abstract class CategoriesPageBase : CrudPage<Models.Category> { }
public abstract class ProductsPageBase   : CrudPage<Models.Product>  { }
public abstract class ClientsPageBase    : CrudPage<Models.Client>   { }
public abstract class StatusesPageBase   : CrudPage<Models.OrderStatus> { }
