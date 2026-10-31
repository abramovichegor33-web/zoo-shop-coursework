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

        ColorBox.ItemsSource = AppColors.Names;
        ColorBox.SelectedItem = AppColors.CurrentName;
        ColorBox.SelectionChanged += (_, _) =>
        {
            if (ColorBox.SelectedItem is string n)
            {
                AppColors.Apply(n);
            }
        };
    }

    private void LoginButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var user = Database.Login(LoginBox.Text ?? "", PasswordBox.Text ?? "");
            if (user == null)
            {
                MessageText.Foreground = Brushes.Red;
                MessageText.Text = "Неверный логин или пароль";
                return;
            }
            Session.Current = user;
            new ProductsWindow().Show();
            Close();
        }
        catch (Exception ex)
        {
            MessageText.Foreground = Brushes.Red;
            MessageText.Text = "Ошибка базы данных: " + ex.Message;
        }
    }
}