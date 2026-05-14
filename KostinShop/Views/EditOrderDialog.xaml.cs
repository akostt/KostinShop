using KostinShop.Helpers;
using KostinShop.Models;
using KostinShop.Services;
using System.Windows;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class EditOrderDialog : DialogBase
{
    private readonly int _orderId;
    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public EditOrderDialog(OrderSummaryDto order)
    {
        InitializeComponent();
        _orderId = order.ID_Order;
        TitleTextBlock.Text = $"Редактирование заказа №{order.ID_Order}";

        // Статусы
        StatusCombo.ItemsSource   = StatusService.GetAllSimple();
        StatusCombo.SelectedValue = order.ID_Order_Status;

        // Адрес
        AddressTextBox.Text       = order.Delivery_Address;

        // Товары для добавления
        AddProductCombo.ItemsSource = ProductService.GetAll();

        // Состав
        RefreshItems();
    }

    private void RefreshItems()
    {
        ItemsGrid.ItemsSource = OrderService.GetItems(_orderId);
    }

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        if (AddProductCombo.SelectedValue is not int productId)
        {
            ShowError("Выберите товар для добавления.");
            return;
        }
        if (!int.TryParse(AddQtyTextBox.Text.Trim(), out int qty) || qty <= 0)
        {
            ShowError("Количество должно быть целым числом больше нуля.");
            return;
        }

        var err = OrderService.AddItem(_orderId, productId, qty);
        if (err != null) { ShowError(err); return; }

        AddProductCombo.SelectedIndex = -1;
        AddQtyTextBox.Text = "1";
        RefreshItems();
    }

    private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        if (ItemsGrid.SelectedItem is not ProductOrder item)
        {
            ShowError("Выберите позицию в таблице для удаления.");
            return;
        }

        var err = OrderService.RemoveItem(_orderId, item.ID_Product);
        if (err != null) { ShowError(err); return; }
        RefreshItems();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        HideError();

        if (StatusCombo.SelectedValue is not int statusId || statusId <= 0)
        { ShowError("Выберите статус."); return; }

        var address = AddressTextBox.Text;

        var currentOrder = OrderService.GetSummary()
            .FirstOrDefault(o => o.ID_Order == _orderId);
        if (currentOrder == null)
        { ShowError("Заказ не найден."); return; }

        SaveAndClose(() => OrderService.UpdateOrder(
            _orderId, currentOrder.ID_Client, statusId, address, currentOrder.Order_date));
    }
}
