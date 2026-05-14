using KostinShop.Helpers;
using KostinShop.Models;
using KostinShop.Services;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class ClientEditDialog : DialogBase
{
    private readonly Client? _client;

    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public ClientEditDialog() => InitializeComponent();

    public ClientEditDialog(Client client) : this()
    {
        _client                   = client;
        TitleTextBlock.Text       = "Редактирование клиента";
        FirstNameTextBox.Text     = client.First_Name;
        MiddleNameTextBox.Text    = client.Middle_Name ?? string.Empty;
        LastNameTextBox.Text      = client.Last_Name;
        PhoneTextBox.Text         = FormatPhone(client.Phone);
        LoyaltyPointsTextBox.Text = client.Loyalty_Points.ToString();
        PhoneInputHelper.Attach(PhoneTextBox);
    }

    private static string FormatPhone(string phone)
    {
        var digits = Regex.Replace(phone, @"\D", "");
        if (digits.Length == 11 && digits.StartsWith("7")) digits = digits[1..];
        return PhoneInputHelper.Format(digits[..Math.Min(10, digits.Length)]);
    }

    private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_client == null) return;

        if (!int.TryParse(LoyaltyPointsTextBox.Text.Trim(), out var pts) || pts < 0)
        {
            ShowError("Бонусные баллы должны быть целым неотрицательным числом.");
            return;
        }

        SaveAndClose(() => ClientService.Update(
            _client.ID_Client,
            _client.First_Name,
            _client.Middle_Name ?? string.Empty,
            LastNameTextBox.Text,
            PhoneTextBox.Text,
            pts));
    }
}
