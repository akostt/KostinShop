using KostinShop.Models;
using KostinShop.Services;
using System.Windows.Controls;

namespace KostinShop.Views;

public partial class CategoryEditDialog : DialogBase
{
    private readonly Category? _existing;

    protected override TextBlock ErrorBlock => ErrorTextBlock;

    public CategoryEditDialog()
    {
        InitializeComponent();
        TitleTextBlock.Text = "Добавление категории";
        Title = "Новая категория — KostinShop";
        NameTextBox.Focus();
    }

    public CategoryEditDialog(Category category) : this()
    {
        _existing           = category;
        TitleTextBlock.Text = "Редактирование категории";
        Title               = "Редактирование категории — KostinShop";
        NameTextBox.Text    = category.Name;
    }

    private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e) =>
        SaveAndClose(() => _existing == null
            ? CategoryService.Add(NameTextBox.Text)
            : CategoryService.Update(_existing.ID_Category, NameTextBox.Text));
}
