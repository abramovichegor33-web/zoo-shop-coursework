using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ZooShop;

public partial class CartWindow : Window
{
    public bool OrderPlaced { get; private set; }

    public CartWindow()
    {
        InitializeComponent();
        Refresh();
    }

    private void Refresh()
    {
        CartGrid.ItemsSource = Session.Cart.ToList();
        var total = Session.Cart.Sum(i => i.Sum);
        TotalText.Text = "Итого: " + total.ToString("N2") + " ₽";
    }

    private void SetStatus(string text, bool ok)
    {
        StatusText.Foreground = ok ? Brushes.Green : Brushes.Red;
        StatusText.Text = text;
    }

    private void PlusButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CartGrid.SelectedItem is not CartItem item)
        {
            SetStatus("Выберите товар в таблице", false);
            return;
        }
        if (item.Quantity >= item.MaxQuantity)
        {
            SetStatus("На складе больше нет", false);
            return;
        }
        item.Quantity++;
        StatusText.Text = "";
        Refresh();
    }

    private void MinusButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CartGrid.SelectedItem is not CartItem item)
        {
            SetStatus("Выберите товар в таблице", false);
            return;
        }
        if (item.Quantity > 1)
        {
            item.Quantity--;
        }
        else
        {
            Session.Cart.Remove(item);
        }
        StatusText.Text = "";
        Refresh();
    }

    private void RemoveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CartGrid.SelectedItem is not CartItem item)
        {
            SetStatus("Выберите товар в таблице", false);
            return;
        }
        Session.Cart.Remove(item);
        StatusText.Text = "";
        Refresh();
    }

    private async void OrderButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Session.Current == null)
        {
            SetStatus("Нужно войти в аккаунт", false);
            return;
        }
        if (Session.Cart.Count == 0)
        {
            SetStatus("Корзина пуста", false);
            return;
        }

        var payment = new PaymentWindow();
        await payment.ShowDialog(this);
        if (payment.Paid)
        {
            OrderPlaced = true;
            Refresh();
            SetStatus("Заказ №" + payment.OrderId + " оформлен", true);
        }
    }
}