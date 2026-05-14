using KostinShop.Models;
using KostinShop.Services;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace KostinShop.Views;

public partial class StatusesPage : StatusesPageBase
{
    protected override DataGrid Grid       => StatusesDataGrid;
    protected override string   EntityName => "статус";

    public StatusesPage()
    {
        InitializeComponent();
        LoadData();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ApplyAdminReadOnly(BtnAdd, BtnEdit, BtnDelete);
    }

    protected override void LoadData()
        => StatusesDataGrid.ItemsSource = StatusService.GetAll();

    protected override void OpenAddDialog()
        => OpenDialogAndRefresh(new StatusEditDialog());

    protected override void OpenEditDialog(OrderStatus item)
        => OpenDialogAndRefresh(new StatusEditDialog(item));

    protected override string DeleteConfirmMessage(OrderStatus item)
        => $"Удалить статус «{item.Name}»?";

    protected override Func<string?> DeleteAction(OrderStatus item)
        => () => StatusService.Delete(item.ID_Order_Status);

    private void RefreshButton_Click(object s, System.Windows.RoutedEventArgs e) => LoadData();
    private void AddButton_Click(object s, System.Windows.RoutedEventArgs e)    => OnAdd(s, e);
    private void EditButton_Click(object s, System.Windows.RoutedEventArgs e)   => OnEdit(s, e);
    private void DeleteButton_Click(object s, System.Windows.RoutedEventArgs e) => OnDelete(s, e);
    private void StatusesDataGrid_MouseDoubleClick(object s, MouseButtonEventArgs e) => OnDoubleClick(s, e);
}
