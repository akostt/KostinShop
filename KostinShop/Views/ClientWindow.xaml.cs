using KostinShop.Helpers;
using KostinShop.Models;
using KostinShop.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KostinShop.Views;

public partial class ClientWindow : Window
{
    private readonly AppUser _user;
    private bool _initialized;

    private int? _selectedCategoryId = null;
    private string? _selectedCategoryName = null;

    private int ClientId => _user.ID_Client
        ?? throw new InvalidOperationException(
            $"Пользователь '{_user.Login}' не является клиентом (ID_Client = null).");

    public ClientWindow(AppUser user)
    {
        if (user.ID_Client == null)
        {
            UiHelper.ShowWarning(
                "Этот пользователь не имеет клиентского профиля.",
                "Нет клиентского профиля");
            throw new InvalidOperationException("Пользователь не имеет клиентского профиля.");
        }

        _user = user;
        InitializeComponent();
        WelcomeTextBlock.Text = $"👤 {user.Last_Name} {user.First_Name}";

        ShowCategoriesView();
        LoadCart();
        LoadOrders();
        _initialized = true;
    }

    private void ShowCategoriesView()
    {
        _selectedCategoryId = null;
        _selectedCategoryName = null;

        CategoriesView.Visibility        = Visibility.Visible;
        ProductsView.Visibility          = Visibility.Collapsed;
        BackToCategoriesButton.Visibility = Visibility.Collapsed;

        CatalogSearchBox.Tag  = "🔍 Поиск по всем товарам...";
        CatalogSearchBox.Text = string.Empty;

        CatalogTitleText.Text    = "🖥️ Каталог";
        CatalogSubtitleText.Text = "Выберите категорию";

        LoadCategoryCards();
    }

    private void LoadCategoryCards()
    {
        CategoriesPanel.Children.Clear();
        var categories = CategoryService.GetAll();

        var allCard = MakeCategoryCard("📦", "Все товары",
            $"{categories.Sum(c => c.ProductCount)} позиций", null);
        CategoriesPanel.Children.Add(allCard);

        foreach (var cat in categories)
        {
            var card = MakeCategoryCard("📁", cat.Name,
                $"{cat.ProductCount} товаров", cat.ID_Category);
            CategoriesPanel.Children.Add(card);
        }
    }

    private Border MakeCategoryCard(string icon, string name, string subtitle, int? categoryId)
    {
        var card = new Border
        {
            Background   = Brushes.White,
            CornerRadius = new CornerRadius(10),
            Margin       = new Thickness(6),
            Cursor       = Cursors.Hand,
            Effect       = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 8, ShadowDepth = 2, Opacity = 0.2,
                Color = Color.FromRgb(0xBB, 0xBB, 0xBB)
            }
        };

        var inner = new Grid();
        var topBar = new Border
        {
            Height       = 6,
            Background   = (SolidColorBrush)FindResource("PrimaryBrush"),
            CornerRadius = new CornerRadius(10, 10, 0, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        inner.Children.Add(topBar);

        var content = new StackPanel
        {
            VerticalAlignment   = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(12, 14, 12, 12)
        };
        content.Children.Add(new TextBlock
        {
            Text              = icon,
            FontSize          = 28,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin            = new Thickness(0, 0, 0, 6)
        });
        content.Children.Add(new TextBlock
        {
            Text                = name,
            FontSize            = 13,
            FontWeight          = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment       = TextAlignment.Center,
            TextWrapping        = TextWrapping.Wrap
        });
        content.Children.Add(new TextBlock
        {
            Text                = subtitle,
            FontSize            = 11,
            Foreground          = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new Thickness(0, 2, 0, 0)
        });
        inner.Children.Add(content);
        card.Child = inner;

        card.Tag = categoryId;
        card.MouseLeftButtonUp += CategoryCard_Click;

        card.MouseEnter += (s, e) =>
            card.Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF8, 0xF6));
        card.MouseLeave += (s, e) =>
            card.Background = Brushes.White;

        return card;
    }

    private void CategoryCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { Tag: var tag }) return;
        var categoryId = tag as int?;
        ShowProductsView(categoryId);
    }

    private void BackToCategories_Click(object sender, RoutedEventArgs e)
        => ShowCategoriesView();

    private void ShowProductsView(int? categoryId)
    {
        _selectedCategoryId = categoryId;

        CategoriesView.Visibility        = Visibility.Collapsed;
        ProductsView.Visibility          = Visibility.Visible;
        BackToCategoriesButton.Visibility = Visibility.Visible;
        CatalogSearchBox.Text            = string.Empty;

        if (categoryId == null)
        {
            _selectedCategoryName            = "Все товары";
            CatalogTitleText.Text            = "📦 Все товары";
            CatalogSubtitleText.Text         = "Весь каталог";
            CatalogSearchBox.Tag             = "🔍 Поиск по всем товарам...";
        }
        else
        {
            var cats = CategoryService.GetAllSimple();
            _selectedCategoryName    = cats.FirstOrDefault(c => c.ID_Category == categoryId)?.Name ?? "Категория";
            CatalogTitleText.Text    = $"📁 {_selectedCategoryName}";
            CatalogSubtitleText.Text = "Товары категории";
            CatalogSearchBox.Tag     = $"🔍 Поиск в «{_selectedCategoryName}»...";
        }

        LoadProductCards(string.Empty);
    }

    private void LoadProductCards(string? search)
    {
        ProductsPanel.Children.Clear();
        var products = ProductService.GetAll(search, _selectedCategoryId);

        if (products.Count == 0)
        {
            ProductsPanel.Children.Add(new TextBlock
            {
                Text       = "Товары не найдены",
                FontSize   = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99)),
                Margin     = new Thickness(16)
            });
            return;
        }

        foreach (var product in products)
            ProductsPanel.Children.Add(MakeProductCard(product));
    }

    private Border MakeProductCard(Product product)
    {
        var card = new Border
        {
            Background   = Brushes.White,
            CornerRadius = new CornerRadius(10),
            Margin       = new Thickness(6),
            Effect       = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 8, ShadowDepth = 2, Opacity = 0.2,
                Color = Color.FromRgb(0xBB, 0xBB, 0xBB)
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var bar = new Border
        {
            Background   = (SolidColorBrush)FindResource("PrimaryDarkBrush"),
            CornerRadius = new CornerRadius(10, 10, 0, 0)
        };
        Grid.SetRow(bar, 0);
        grid.Children.Add(bar);

        var content = new StackPanel { Margin = new Thickness(12, 10, 12, 6) };

        content.Children.Add(new TextBlock
        {
            Text         = product.Name,
            FontSize     = 13,
            FontWeight   = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight    = 40
        });

        if (!string.IsNullOrWhiteSpace(product.Description))
            content.Children.Add(new TextBlock
            {
                Text         = product.Description,
                FontSize     = 11,
                Foreground   = new SolidColorBrush(Color.FromRgb(0x77, 0x77, 0x77)),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight    = 32,
                Margin       = new Thickness(0, 3, 0, 0)
            });

        content.Children.Add(new TextBlock
        {
            Text       = $"{product.Price:N2} ₽",
            FontSize   = 15,
            FontWeight = FontWeights.Bold,
            Foreground = (SolidColorBrush)FindResource("PrimaryDarkBrush"),
            Margin     = new Thickness(0, 6, 0, 0)
        });

        Grid.SetRow(content, 1);
        grid.Children.Add(content);

        var btn = new Button
        {
            Content             = "+ В корзину",
            Style               = (Style)FindResource("PrimaryButtonStyle"),
            Margin              = new Thickness(12, 0, 12, 10),
            Height              = 30,
            Tag                 = product.ID_Product,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        btn.Click += AddToCart_Click;
        Grid.SetRow(btn, 2);
        grid.Children.Add(btn);

        card.Child = grid;
        return card;
    }

    private void CatalogSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_initialized) return;

        var text = CatalogSearchBox.Text;

        if (ProductsView.Visibility == Visibility.Visible)
        {
            LoadProductCards(text);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                CategoriesView.Visibility        = Visibility.Collapsed;
                ProductsView.Visibility          = Visibility.Visible;
                BackToCategoriesButton.Visibility = Visibility.Visible;
                CatalogTitleText.Text            = "🔍 Результаты поиска";
                CatalogSubtitleText.Text         = $"По запросу «{text}»";
                _selectedCategoryId              = null;
                _selectedCategoryName            = null;
                LoadProductCards(text);
            }
            else
            {
                ShowCategoriesView();
            }
        }
    }

    // Оставляем для совместимости (поля больше нет в XAML, но могут быть ссылки)
    private void SearchTextBox_TextChanged(object s, TextChangedEventArgs e) { }
    private void ProductSearchBox_TextChanged(object s, TextChangedEventArgs e) { }
    private void CategoryFilter_Changed(object s, SelectionChangedEventArgs e) { }

    private void ResetFilter_Click(object s, RoutedEventArgs e)
        => ShowCategoriesView();

    private void AddToCart_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int productId }) return;
        if (!UiHelper.TryRun(() => CartService.AddToCart(ClientId, productId))) return;
        LoadCart();
        UiHelper.ShowInfo("Товар добавлен в корзину.", "Корзина");
    }

    private void LoadCart()
    {
        var items = CartService.GetCart(ClientId);
        CartDataGrid.ItemsSource = items;
        RefreshCartSummary(items);
        UpdateCartBadge(items.Count);
    }

    private void UpdateCartBadge(int count)
    {
        CartBadge.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        CartCountText.Text   = count.ToString();
    }

    private void RefreshCartSummary(List<Cart>? items = null)
    {
        items ??= CartService.GetCart(ClientId);
        var subtotal = items.Sum(i => i.Total);
        var points   = ParsePointsInput();
        var balance  = CartService.GetLoyaltyPoints(ClientId);
        var spend    = Math.Min(points, Math.Min(balance, (int)subtotal));

        CartTotalTextBlock.Text      = $"Итого: {subtotal - spend:N2} ₽";
        LoyaltyBalanceTextBlock.Text = $"(доступно: {balance} балл(ов), 1 балл = 1 ₽)";

        DiscountTextBlock.Visibility = spend > 0 ? Visibility.Visible : Visibility.Collapsed;
        if (spend > 0)
            DiscountTextBlock.Text = $"Товаров на {subtotal:N2} ₽ − скидка {spend} балл(ов)";
    }

    private int ParsePointsInput()
        => int.TryParse(PointsToSpendTextBox?.Text, out var v) && v >= 0 ? v : 0;

    private void PointsToSpend_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_initialized) return;
        var box      = (TextBox)sender;
        var filtered = new string(box.Text.Where(char.IsDigit).ToArray());
        if (filtered != box.Text) { box.Text = filtered; box.CaretIndex = filtered.Length; return; }
        RefreshCartSummary();
    }

    private void IncreaseQty_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int cartId }) return;
        var item = GetCartItem(cartId);
        if (item == null) return;
        CartService.UpdateQuantity(cartId, item.Quantity + 1);
        LoadCart();
    }

    private void DecreaseQty_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int cartId }) return;
        var item = GetCartItem(cartId);
        if (item == null) return;
        if (item.Quantity <= 1 && !UiHelper.Confirm("Убрать товар из корзины?")) return;
        CartService.UpdateQuantity(cartId, item.Quantity - 1);
        LoadCart();
    }

    private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int cartId }) return;
        if (!UiHelper.Confirm("Убрать товар из корзины?")) return;
        CartService.Remove(cartId);
        LoadCart();
    }

    private Cart? GetCartItem(int cartId)
        => (CartDataGrid.ItemsSource as IEnumerable<Cart>)?.FirstOrDefault(i => i.ID_Cart == cartId);

    private void PlaceOrder_Click(object sender, RoutedEventArgs e)
    {
        var points = ParsePointsInput();
        var err = CartService.PlaceOrder(ClientId, DeliveryAddressTextBox.Text, points, out var actualSpend);
        if (err != null) { UiHelper.ShowError(err, "Ошибка оформления"); return; }

        var msg = actualSpend > 0
            ? $"Заказ оформлен! Списано {actualSpend} балл(ов).\nОтследите его в разделе «Мои заказы»."
            : "Заказ успешно оформлен!\nОтследите его в разделе «Мои заказы».";

        UiHelper.ShowInfo(msg, "Заказ оформлен");
        DeliveryAddressTextBox.Clear();
        PointsToSpendTextBox.Text = "0";
        LoadCart();
        LoadOrders();
    }

    private void LoadOrders()
    {
        MyOrdersDataGrid.ItemsSource     = OrderService.GetByClient(ClientId);
        MyOrderItemsDataGrid.ItemsSource = null;
    }

    private void MyOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        MyOrderItemsDataGrid.ItemsSource =
            MyOrdersDataGrid.SelectedItem is OrderSummaryDto selected
                ? OrderService.GetItems(selected.ID_Order)
                : null;
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (!UiHelper.Confirm("Вы уверены, что хотите выйти?", "Подтверждение выхода")) return;
        AuthService.Logout();
        new LoginWindow().Show();
        Close();
    }
}
