using KostinShop.Helpers;
using KostinShop.Models;
using KostinShop.Services;
using System.Windows;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class ManageOrderItemsDialog : DialogBase
{
    private readonly int _orderId;
    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public ManageOrderItemsDialog(OrderSummaryDto order)
    {
        InitializeComponent();
        _orderId = order.ID_Order;
        TitleTextBlock.Text = $"Состав заказа №{order.ID_Order} — {order.ClientFullName}";
        AddProductCombo.ItemsSource = ProductService.GetAll();
        RefreshItems();
    }

    private void RefreshItems()
        => ItemsGrid.ItemsSource = OrderService.GetItems(_orderId);

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        HideError();
        if (AddProductCombo.SelectedValue is not int productId)
        { ShowError("Выберите товар."); return; }

        if (!int.TryParse(AddQtyTextBox.Text.Trim(), out int qty) || qty <= 0)
        { ShowError("Количество должно быть целым числом больше нуля."); return; }

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
        { ShowError("Выберите позицию для удаления."); return; }

        var err = OrderService.RemoveItem(_orderId, item.ID_Product);
        if (err != null) { ShowError(err); return; }
        RefreshItems();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();
}
