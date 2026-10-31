using Avalonia.Controls;

namespace ZooShop;

public partial class OrdersWindow : Window
{
    public OrdersWindow()
    {
        InitializeComponent();
        if (Session.Current != null)
        {
            OrdersGrid.ItemsSource = Database.GetOrders(Session.Current.Id);
        }
    }
}