using KostinShop.Helpers;
using KostinShop.Models;
using KostinShop.Services;
using System.Windows;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class ProcessOrderDialog : DialogBase
{
    private readonly OrderSummaryDto _order;
    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public ProcessOrderDialog(OrderSummaryDto order)
    {
        InitializeComponent();
        _order = order;
        TitleTextBlock.Text = $"Оформление заказа №{order.ID_Order}";
        ClientTextBlock.Text = order.ClientFullName;
        AddressTextBlock.Text = order.Delivery_Address;
        DateTextBlock.Text = order.Order_date.ToString("dd.MM.yyyy HH:mm");
        StatusTextBlock.Text = order.Status;
        ItemsGrid.ItemsSource = OrderService.GetItems(order.ID_Order);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();

    private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        if (!UiHelper.Confirm(
                $"Отменить заказ №{_order.ID_Order} от {_order.Order_date:dd.MM.yyyy}?\nСтатус будет изменён на «Отменён».",
                "Подтверждение отмены")) return;

        if (!UiHelper.TryRun(() => OrderService.CancelOrder(_order.ID_Order), "Ошибка отмены заказа")) return;
        DialogResult = true;
        Close();
    }

    private void ProcessOrderButton_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        if (!UiHelper.Confirm(
                $"Оформить заказ №{_order.ID_Order} от {_order.Order_date:dd.MM.yyyy}?\nСтатус будет изменён на «Оформлен».",
                "Подтверждение оформления")) return;

        if (!UiHelper.TryRun(() => OrderService.MarkAsProcessed(_order.ID_Order), "Ошибка оформления заказа")) return;
        DialogResult = true;
        Close();
    }
}
