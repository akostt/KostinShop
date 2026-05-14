using KostinShop.Services;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class ChangeDeliveryAddressDialog : DialogBase
{
    private readonly int _orderId;

    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public ChangeDeliveryAddressDialog(int orderId, string currentAddress)
    {
        InitializeComponent();
        _orderId              = orderId;
        OrderIdTextBlock.Text = orderId.ToString();
        AddressTextBox.Text   = currentAddress;
    }

    private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e) =>
        SaveAndClose(() => OrderService.ChangeDeliveryAddress(_orderId, AddressTextBox.Text));
}
