using KostinShop.Models;
using KostinShop.Services;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace KostinShop.Views;

public partial class ProductsPage : ProductsPageBase
{
    protected override DataGrid Grid       => ProductsDataGrid;
    protected override string   EntityName => "товар";

    public ProductsPage()
    {
        InitializeComponent();
        LoadCategoryFilter();
        LoadData();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ApplyAdminReadOnly(BtnAdd, BtnEdit, BtnDelete);
    }

    private void LoadCategoryFilter()
    {
        var cats = CategoryService.GetAllSimple();
        cats.Insert(0, new Category { ID_Category = 0, Name = "— Все категории —" });
        CategoryFilterCombo.ItemsSource       = cats;
        CategoryFilterCombo.DisplayMemberPath = "Name";
        CategoryFilterCombo.SelectedValuePath = "ID_Category";
        CategoryFilterCombo.SelectedIndex     = 0;
    }

    protected override void LoadData()
    {
        var catId = CategoryFilterCombo?.SelectedValue as int?;
        ProductsDataGrid.ItemsSource = ProductService.GetAll(
            SearchTextBox?.Text,
            catId > 0 ? catId : null);
    }

    protected override void OpenAddDialog()
        => OpenDialogAndRefresh(new ProductEditDialog());

    protected override void OpenEditDialog(Product item)
        => OpenDialogAndRefresh(new ProductEditDialog(item));

    protected override string DeleteConfirmMessage(Product item)
        => $"Удалить товар «{item.Name}»?";

    protected override Func<string?> DeleteAction(Product item)
        => () => ProductService.Delete(item.ID_Product);

    private void SearchTextBox_TextChanged(object s, System.Windows.Controls.TextChangedEventArgs e) => LoadData();
    private void CategoryFilter_Changed(object s, System.Windows.Controls.SelectionChangedEventArgs e) => LoadData();
    private void RefreshButton_Click(object s, System.Windows.RoutedEventArgs e) { SearchTextBox.Clear(); CategoryFilterCombo.SelectedIndex = 0; LoadData(); }
    private void AddButton_Click(object s, System.Windows.RoutedEventArgs e)    => OnAdd(s, e);
    private void EditButton_Click(object s, System.Windows.RoutedEventArgs e)   => OnEdit(s, e);
    private void DeleteButton_Click(object s, System.Windows.RoutedEventArgs e) => OnDelete(s, e);
    private void ProductsDataGrid_MouseDoubleClick(object s, MouseButtonEventArgs e) => OnDoubleClick(s, e);
}
