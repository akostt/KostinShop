using KostinShop.Models;
using KostinShop.Services;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace KostinShop.Views;

public partial class CategoriesPage : CategoriesPageBase
{
    protected override DataGrid Grid       => CategoriesDataGrid;
    protected override string   EntityName => "категорию";

    public CategoriesPage()
    {
        InitializeComponent();
        LoadData();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ApplyAdminReadOnly(BtnAdd, BtnEdit, BtnDelete);
    }

    protected override void LoadData()
        => CategoriesDataGrid.ItemsSource = CategoryService.GetAll(SearchTextBox?.Text);

    protected override void OpenAddDialog()
        => OpenDialogAndRefresh(new CategoryEditDialog());

    protected override void OpenEditDialog(Category item)
        => OpenDialogAndRefresh(new CategoryEditDialog(item));

    protected override string DeleteConfirmMessage(Category item)
        => $"Удалить категорию «{item.Name}»?\n\nЭто действие невозможно отменить.";

    protected override Func<string?> DeleteAction(Category item)
        => () => CategoryService.Delete(item.ID_Category);

    private void SearchTextBox_TextChanged(object s, System.Windows.Controls.TextChangedEventArgs e) => LoadData();
    private void RefreshButton_Click(object s, System.Windows.RoutedEventArgs e) { SearchTextBox.Clear(); LoadData(); }
    private void AddButton_Click(object s, System.Windows.RoutedEventArgs e)    => OnAdd(s, e);
    private void EditButton_Click(object s, System.Windows.RoutedEventArgs e)   => OnEdit(s, e);
    private void DeleteButton_Click(object s, System.Windows.RoutedEventArgs e) => OnDelete(s, e);
    private void CategoriesDataGrid_MouseDoubleClick(object s, MouseButtonEventArgs e) => OnDoubleClick(s, e);
}
