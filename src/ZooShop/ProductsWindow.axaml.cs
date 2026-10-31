using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;

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

        var categories = new List<string> { AllCategories };
        categories.AddRange(Database.GetCategories());
        CategoryBox.ItemsSource = categories;
        CategoryBox.SelectedIndex = 0;

        AddCategoryBox.ItemsSource = Database.GetCategories();
        AddCategoryBox.SelectedIndex = 0;

        LoadProducts();

        SearchBox.TextChanged += (_, _) => ApplyFilter();
        CategoryBox.SelectionChanged += (_, _) => ApplyFilter();
    }

    private void RefreshHeader()
    {
        var roleName = _user.IsAdmin ? "администратор" : "покупатель";
        UserText.Text = _user.FullName + " (" + roleName + ")";
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

    private void ThemeButton_Click(object? sender, RoutedEventArgs e)
    {
        var app = Application.Current!;
        app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
    }

    private async void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        await new SettingsWindow().ShowDialog(this);
        RefreshHeader();
    }

    private void LogoutButton_Click(object? sender, RoutedEventArgs e)
    {
        Session.Current = null;
        new MainWindow().Show();
        Close();
    }
}
