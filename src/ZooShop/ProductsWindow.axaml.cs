using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ZooShop;

public partial class ProductsWindow : Window
{
    private const string AllCategories = "Все категории";
    private readonly User _user;
    private List<Product> _all = new();

    public ProductsWindow()
    {
        InitializeComponent();
        _user = Session.Current ?? new User { Login = "guest", FullName = "Гость", Role = "user" };
        RefreshHeader();
        AdminPanel.IsVisible = _user.IsAdmin;
        AddToCartButton.IsVisible = !_user.IsAdmin;
        CartButton.IsVisible = !_user.IsAdmin;
        OrdersButton.IsVisible = !_user.IsAdmin;
        UpdateCartButton();

        var categories = new List<string> { AllCategories };
        categories.AddRange(Database.GetCategories());
        CategoryBox.ItemsSource = categories;
        CategoryBox.SelectedIndex = 0;

        AddCategoryBox.ItemsSource = Database.GetCategories();
        AddCategoryBox.SelectedIndex = 0;

        ThemePicker.Attach(ColorButton);
        LoadProducts();

        SearchBox.TextChanged += (_, _) => ApplyFilter();
        CategoryBox.SelectionChanged += (_, _) => ApplyFilter();
    }

    private void RefreshHeader()
    {
        var roleName = _user.IsAdmin ? "администратор" : "покупатель";
        UserText.Text = _user.FullName + " (" + roleName + ")";
    }

    private void UpdateCartButton()
    {
        var count = Session.Cart.Sum(i => i.Quantity);
        CartButton.Content = count > 0 ? "Корзина (" + count + ")" : "Корзина";
    }

    private void LoadProducts()
    {
        _all = Database.GetProducts();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var text = (SearchBox.Text ?? "").Trim();
        var category = CategoryBox.SelectedItem as string ?? AllCategories;

        var result = _all.Where(p =>
            (text == "" || p.Name.Contains(text, StringComparison.OrdinalIgnoreCase)) &&
            (category == AllCategories || p.Category == category));

        ProductsGrid.ItemsSource = result.ToList();
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        SearchBox.Text = "";
        CategoryBox.SelectedIndex = 0;
    }

    private void SetStatus(string text, bool ok)
    {
        StatusText.Foreground = ok ? Brushes.Green : Brushes.Red;
        StatusText.Text = text;
    }

    private void SetCartStatus(string text, bool ok)
    {
        CartStatusText.Foreground = ok ? Brushes.Green : Brushes.Red;
        CartStatusText.Text = text;
    }

    private void AddToCartButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not Product product)
        {
            SetCartStatus("Выберите товар в таблице", false);
            return;
        }

        var existing = Session.Cart.FirstOrDefault(i => i.ProductId == product.Id);
        var inCart = existing?.Quantity ?? 0;
        if (inCart >= product.Quantity)
        {
            SetCartStatus(product.Quantity == 0 ? "Товара нет в наличии" : "Больше нет на складе", false);
            return;
        }

        if (existing == null)
        {
            Session.Cart.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1,
                MaxQuantity = product.Quantity
            });
        }
        else
        {
            existing.Quantity++;
            existing.MaxQuantity = product.Quantity;
        }

        UpdateCartButton();
        SetCartStatus("Добавлено: " + product.Name, true);
    }

    private async void CartButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new CartWindow();
        await window.ShowDialog(this);
        UpdateCartButton();
        if (window.OrderPlaced)
        {
            LoadProducts();
            SetCartStatus("Заказ оформлен", true);
        }
    }

    private async void OrdersButton_Click(object? sender, RoutedEventArgs e)
    {
        await new OrdersWindow().ShowDialog(this);
    }

    private void AddButton_Click(object? sender, RoutedEventArgs e)
    {
        var name = (NameBox.Text ?? "").Trim();
        var category = AddCategoryBox.SelectedItem as string;
        var priceText = (PriceBox.Text ?? "").Trim().Replace(',', '.');

        if (name == "" || category == null)
        {
            SetStatus("Введите название и выберите категорию", false);
            return;
        }
        if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price < 0)
        {
            SetStatus("Цена указана неверно", false);
            return;
        }
        if (!int.TryParse((QuantityBox.Text ?? "").Trim(), out var quantity) || quantity < 0)
        {
            SetStatus("Количество указано неверно", false);
            return;
        }

        try
        {
            Database.AddProduct(name, category, price, quantity, (DescriptionBox.Text ?? "").Trim());
            NameBox.Text = "";
            PriceBox.Text = "";
            QuantityBox.Text = "";
            DescriptionBox.Text = "";
            LoadProducts();
            SetStatus("Товар добавлен", true);
        }
        catch (Exception ex)
        {
            SetStatus("Ошибка: " + ex.Message, false);
        }
    }

    private void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not Product product)
        {
            SetStatus("Выберите товар в таблице", false);
            return;
        }

        try
        {
            Database.DeleteProduct(product.Id);
            LoadProducts();
            SetStatus("Товар удалён: " + product.Name, true);
        }
        catch (Exception ex)
        {
            SetStatus("Ошибка: " + ex.Message, false);
        }
    }

    private async void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        await new SettingsWindow().ShowDialog(this);
        RefreshHeader();
    }

    private void LogoutButton_Click(object? sender, RoutedEventArgs e)
    {
        Session.Current = null;
        Session.Cart.Clear();
        new MainWindow().Show();
        Close();
    }
}