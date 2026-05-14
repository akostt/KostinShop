using KostinShop.Helpers;
using KostinShop.Services;
using System.Windows;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class OrdersPage : Page
{
    public OrdersPage()
    {
        InitializeComponent();

        UiHelper.SetVisibility(AuthService.IsManager, EditOrderButton, DeleteButton, ExportOrderButton);
        UiHelper.SetVisibility(AuthService.IsLogist  && !AuthService.IsManager, ChangeStatusButton, ChangeAddressButton);

        LoadStatuses();

        if (AuthService.IsManager)
        {
            StatusFilterCombo.SelectedValue = (StatusFilterCombo.ItemsSource as IEnumerable<Models.OrderStatus>)
                ?.FirstOrDefault(s => s.Name == "Новый")?.ID_Order_Status ?? 0;
        }

        LoadData();
    }

    private void LoadStatuses()
    {
        var statuses = StatusService.GetAllSimple();
        statuses.Insert(0, new Models.OrderStatus { ID_Order_Status = 0, Name = "— Все статусы —" });
        StatusFilterCombo.ItemsSource       = statuses;
        StatusFilterCombo.DisplayMemberPath = "Name";
        StatusFilterCombo.SelectedValuePath = "ID_Order_Status";
        StatusFilterCombo.SelectedIndex     = 0;
    }

    private void LoadData()
    {
        var statusId = StatusFilterCombo?.SelectedValue as int?;
        OrdersDataGrid.ItemsSource     = OrderService.GetSummary(statusId > 0 ? statusId : null);
        OrderItemsDataGrid.ItemsSource = null;
        UpdateProcessButtonVisibility();
    }

    private OrderSummaryDto? SelectedOrder
        => OrdersDataGrid.SelectedItem as OrderSummaryDto;

    private bool RequireSelection(string action)
    {
        if (SelectedOrder != null) return true;
        UiHelper.ShowWarning($"Выберите заказ для действия «{action}».", "Ничего не выбрано");
        return false;
    }

    private void StatusFilter_Changed(object s, SelectionChangedEventArgs e)  => LoadData();
    private void RefreshButton_Click(object s, RoutedEventArgs e) { StatusFilterCombo.SelectedIndex = 0; LoadData(); }

    private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        OrderItemsDataGrid.ItemsSource = SelectedOrder is { } sel
            ? OrderService.GetItems(sel.ID_Order)
            : null;

        UpdateProcessButtonVisibility();

    }

    private void UpdateProcessButtonVisibility()
    {
        ProcessOrderButton.Visibility = AuthService.IsManager && SelectedOrder is { Status: "Новый" }
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void OrdersDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!AuthService.IsManager || SelectedOrder is not { Status: "Новый" } selected) return;
        var dlg = new ProcessOrderDialog(selected) { Owner = Window.GetWindow(this) };
        dlg.ShowDialog();
        LoadData();
    }

    private void ProcessOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireSelection("Оформить заказ")) return;
        var sel = SelectedOrder!;

        if (sel.Status != "Новый")
        {
            UiHelper.ShowWarning("Оформить можно только заказы со статусом «Новый».", "Недоступно");
            return;
        }

        var dlg = new ProcessOrderDialog(sel) { Owner = Window.GetWindow(this) };
        dlg.ShowDialog();
        LoadData();
    }

    private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireSelection("Сменить статус")) return;
        var sel = SelectedOrder!;
        var dlg = new ChangeOrderStatusDialog(sel.ID_Order, sel.Status) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) LoadData();
    }

    private void ChangeAddressButton_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireSelection("Изменить адрес")) return;
        var sel = SelectedOrder!;
        var dlg = new ChangeDeliveryAddressDialog(sel.ID_Order, sel.Delivery_Address) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) LoadData();
    }

    private void ManageItemsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireSelection("Управлять позициями")) return;
        var sel = SelectedOrder!;
        var dlg = new ManageOrderItemsDialog(sel) { Owner = Window.GetWindow(this) };
        dlg.ShowDialog();
        OrderItemsDataGrid.ItemsSource = OrderService.GetItems(sel.ID_Order);
    }

    private void EditOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireSelection("Редактировать заказ")) return;
        var sel = SelectedOrder!;
        var dlg = new EditOrderDialog(sel) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) LoadData();
    }

    private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireSelection("Отменить заказ")) return;
        var sel = SelectedOrder!;

        if (!UiHelper.Confirm(
                $"Отменить заказ №{sel.ID_Order} от {sel.Order_date:dd.MM.yyyy}?\nСтатус будет изменён на «Отменён».",
                "Подтверждение отмены")) return;

        if (UiHelper.TryRun(() => OrderService.CancelOrder(sel.ID_Order), "Ошибка отмены заказа"))
            LoadData();
    }

    private void ExportOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireSelection("Экспортировать")) return;
        var sel = SelectedOrder!;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title    = $"Сохранить заказ №{sel.ID_Order}",
            Filter   = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"KostinShop_Order_{sel.ID_Order}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
        if (dialog.ShowDialog() != true) return;

        if (UiHelper.TryRun(() => { ReportService.GenerateSingleOrderReport(sel.ID_Order, dialog.FileName); return null; }, "Ошибка экспорта заказа"))
            UiHelper.ShowInfo($"Заказ успешно экспортирован:\n{dialog.FileName}", "Экспорт выполнен");
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireSelection("Удалить")) return;
        var sel = SelectedOrder!;

        if (!UiHelper.Confirm(
                $"Удалить заказ №{sel.ID_Order} от {sel.Order_date:dd.MM.yyyy}?\nВсе позиции заказа будут также удалены.",
                "Подтверждение удаления")) return;

        if (UiHelper.TryRun(() => OrderService.Delete(sel.ID_Order), "Ошибка удаления"))
            LoadData();
    }
}
