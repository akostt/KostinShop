using KostinShop.Services;
using System.Windows;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class ChangeOrderStatusDialog : DialogBase
{
    private readonly int _orderId;

    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public ChangeOrderStatusDialog(int orderId, string currentStatus)
    {
        InitializeComponent();
        _orderId                = orderId;
        OrderInfoTextBlock.Text = $"Заказ №{orderId} — текущий статус: {currentStatus}";
        StatusCombo.ItemsSource = StatusService.GetAllSimple();
        Title                   = $"Статус заказа №{orderId} — KostinShop";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (StatusCombo.SelectedValue is not int statusId || statusId <= 0)
        {
            ShowError("Выберите новый статус.");
            return;
        }
        SaveAndClose(() => OrderService.ChangeStatus(_orderId, statusId));
    }
}
