using Avalonia.Controls;

namespace ZooShop;

public partial class ProductsWindow : Window
{
    private readonly string _role;

    public ProductsWindow() : this("user")
    {
    }

    public ProductsWindow(string role)
    {
        InitializeComponent();
        _role = role;
        UserText.Text = "Роль: " + _role;
        ProductsGrid.ItemsSource = Database.GetProducts();
    }
}