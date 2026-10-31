using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ZooShop;

public partial class PaymentWindow : Window
{
    private const string ByCard = "Банковской картой";
    private const string BySbp = "СБП";
    private const string OnReceive = "При получении";

    public bool Paid { get; private set; }
    public int OrderId { get; private set; }

    public PaymentWindow()
    {
        InitializeComponent();

        var total = Session.Cart.Sum(i => i.Sum);
        SumText.Text = "К оплате: " + total.ToString("N2") + " ₽";

        MethodBox.ItemsSource = new[] { ByCard, BySbp, OnReceive };
        MethodBox.SelectedIndex = 0;
        MethodBox.SelectionChanged += (_, _) => UpdateMethod();
        UpdateMethod();
    }

    private void UpdateMethod()
    {
        var method = MethodBox.SelectedItem as string;
        CardPanel.IsVisible = method == ByCard;
        InfoText.IsVisible = method != ByCard;
        if (method == BySbp)
        {
            InfoText.Text = "Нажмите «Оплатить», чтобы имитировать подтверждение платежа в приложении банка.";
            PayButton.Content = "Оплатить";
        }
        else if (method == OnReceive)
        {
            InfoText.Text = "Заказ будет оформлен, оплата при получении.";
            PayButton.Content = "Оформить заказ";
        }
        else
        {
            PayButton.Content = "Оплатить";
        }
        StatusText.Text = "";
    }

    private void SetStatus(string text, bool ok)
    {
        StatusText.Foreground = ok ? Brushes.Green : Brushes.Red;
        StatusText.Text = text;
    }

    private string? Validate()
    {
        var digits = new string((CardBox.Text ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length != 16)
        {
            return "Номер карты должен содержать 16 цифр";
        }
        if (string.IsNullOrWhiteSpace(HolderBox.Text))
        {
            return "Укажите владельца карты";
        }
        var expiry = (ExpiryBox.Text ?? "").Trim();
        if (expiry.Length != 5 || expiry[2] != '/' ||
            !int.TryParse(expiry.Substring(0, 2), out var month) ||
            !int.TryParse(expiry.Substring(3, 2), out var year) ||
            month < 1 || month > 12)
        {
            return "Срок действия в формате ММ/ГГ";
        }
        var now = DateTime.Now;
        if (2000 + year < now.Year || (2000 + year == now.Year && month < now.Month))
        {
            return "Срок действия карты истёк";
        }
        var cvv = CvvBox.Text ?? "";
        if (cvv.Length != 3 || !cvv.All(char.IsDigit))
        {
            return "CVV состоит из 3 цифр";
        }
        return null;
    }

    private async void PayButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Session.Current == null)
        {
            SetStatus("Нужно войти в аккаунт", false);
            return;
        }

        var method = MethodBox.SelectedItem as string ?? ByCard;
        if (method == ByCard)
        {
            var error = Validate();
            if (error != null)
            {
                SetStatus(error, false);
                return;
            }
        }

        PayButton.IsEnabled = false;
        CancelButton.IsEnabled = false;
        MethodBox.IsEnabled = false;
        SetStatus(method == OnReceive ? "Оформляем заказ..." : "Обработка платежа...", true);

        try
        {
            await Task.Delay(2000);
            OrderId = Database.PlaceOrder(Session.Current.Id, Session.Cart.ToList(), method);
            Session.Cart.Clear();
            Paid = true;
            SetStatus((method == OnReceive ? "Заказ №" : "Оплата прошла успешно. Заказ №") + OrderId + " оформлен", true);
            await Task.Delay(1500);
            Close();
        }
        catch (Exception ex)
        {
            SetStatus("Ошибка: " + ex.Message, false);
            PayButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
            MethodBox.IsEnabled = true;
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}