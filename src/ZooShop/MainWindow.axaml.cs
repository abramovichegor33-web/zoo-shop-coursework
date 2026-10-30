using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ZooShop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var role = Database.Login(LoginBox.Text ?? "", PasswordBox.Text ?? "");
            if (role == null)
            {
                MessageText.Foreground = Brushes.Red;
                MessageText.Text = "Неверный логин или пароль";
                return;
            }
            var productsWindow = new ProductsWindow(role);
            productsWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            MessageText.Foreground = Brushes.Red;
            MessageText.Text = "Ошибка базы данных: " + ex.Message;
        }
    }
}