using KostinShop.Models;
using KostinShop.Services;
using System.Globalization;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class ProductEditDialog : DialogBase
{
    private readonly Product? _existing;

    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public ProductEditDialog()
    {
        InitializeComponent();
        CategoryCombo.ItemsSource = CategoryService.GetAllSimple();
        TitleTextBlock.Text       = "Добавление товара";
        Title                     = "Новый товар — KostinShop";
        NameTextBox.Focus();
    }

    public ProductEditDialog(Product product) : this()
    {
        _existing               = product;
        TitleTextBlock.Text     = "Редактирование товара";
        Title                   = "Редактирование товара — KostinShop";
        NameTextBox.Text        = product.Name;
        PriceTextBox.Text       = product.Price.ToString("F2");
        DescriptionTextBox.Text = product.Description;
        CategoryCombo.SelectedValue = product.ID_Category;
    }

    private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        HideError();

        if (!decimal.TryParse(
                PriceTextBox.Text.Replace(',', '.'),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var price))
        {
            ShowError("Введите корректную цену (числовое значение).");
            PriceTextBox.Focus();
            return;
        }

        var categoryId = CategoryCombo.SelectedValue as int? ?? 0;

        SaveAndClose(() => _existing == null
            ? ProductService.Add(NameTextBox.Text, categoryId, price, DescriptionTextBox.Text)
            : ProductService.Update(_existing.ID_Product, NameTextBox.Text, categoryId, price, DescriptionTextBox.Text));
    }
}
