using KostinShop.Models;
using KostinShop.Services;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class StatusEditDialog : DialogBase
{
    private readonly OrderStatus? _existing;

    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public StatusEditDialog()
    {
        InitializeComponent();
        TitleTextBlock.Text = "Добавление статуса";
        Title = "Новый статус — KostinShop";
        NameTextBox.Focus();
    }

    public StatusEditDialog(OrderStatus status) : this()
    {
        _existing           = status;
        TitleTextBlock.Text = "Редактирование статуса";
        Title               = "Редактирование статуса — KostinShop";
        NameTextBox.Text    = status.Name;
    }

    private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e) =>
        SaveAndClose(() => _existing == null
            ? StatusService.Add(NameTextBox.Text)
            : StatusService.Update(_existing.ID_Order_Status, NameTextBox.Text));
}
