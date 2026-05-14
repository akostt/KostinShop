using KostinShop.Models;
using KostinShop.Services;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace KostinShop.Views;

public partial class ClientsPage : ClientsPageBase
{
    protected override DataGrid Grid       => ClientsDataGrid;
    protected override string   EntityName => "клиента";

    public ClientsPage()
    {
        InitializeComponent();
        LoadData();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ApplyAdminReadOnly(BtnEdit, BtnDelete);
    }

    protected override void LoadData()
        => ClientsDataGrid.ItemsSource = ClientService.GetAll(SearchTextBox?.Text);

    protected override void OpenAddDialog() { }

    protected override void OpenEditDialog(Client item)
        => OpenDialogAndRefresh(new ClientEditDialog(item));

    protected override string DeleteConfirmMessage(Client item)
        => $"Удалить клиента «{item.FullName}»?\nБудут удалены все связанные данные (корзина, учётная запись).";

    protected override Func<string?> DeleteAction(Client item)
        => () => ClientService.Delete(item.ID_Client);

    private void SearchTextBox_TextChanged(object s, System.Windows.Controls.TextChangedEventArgs e) => LoadData();
    private void RefreshButton_Click(object s, System.Windows.RoutedEventArgs e) { SearchTextBox.Clear(); LoadData(); }
    private void EditButton_Click(object s, System.Windows.RoutedEventArgs e)   => OnEdit(s, e);
    private void DeleteButton_Click(object s, System.Windows.RoutedEventArgs e) => OnDelete(s, e);
    private void ClientsDataGrid_MouseDoubleClick(object s, MouseButtonEventArgs e) => OnDoubleClick(s, e);
}
