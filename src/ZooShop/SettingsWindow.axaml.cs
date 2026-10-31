using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ZooShop;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        var user = Session.Current;
        LoginText.Text = "Логин: " + (user?.Login ?? "");
        FullNameBox.Text = user?.FullName ?? "";
    }

    private void Show(string text, bool ok)
    {
        MessageText.Foreground = ok ? Brushes.Green : Brushes.Red;
        MessageText.Text = text;
    }

    private void SaveName_Click(object? sender, RoutedEventArgs e)
    {
        var user = Session.Current;
        var name = (FullNameBox.Text ?? "").Trim();
        if (user == null || name == "")
        {
            Show("Введите имя", false);
            return;
        }

        try
        {
            Database.UpdateFullName(user.Id, name);
            user.FullName = name;
            Show("Имя сохранено", true);
        }
        catch (Exception ex)
        {
            Show("Ошибка: " + ex.Message, false);
        }
    }

    private void ChangePassword_Click(object? sender, RoutedEventArgs e)
    {
        var user = Session.Current;
        var oldPassword = OldPasswordBox.Text ?? "";
        var newPassword = NewPasswordBox.Text ?? "";
        if (user == null)
        {
            return;
        }
        if (newPassword.Length < 4)
        {
            Show("Новый пароль слишком короткий", false);
            return;
        }

        try
        {
            if (Database.ChangePassword(user.Id, oldPassword, newPassword))
            {
                OldPasswordBox.Text = "";
                NewPasswordBox.Text = "";
                Show("Пароль изменён", true);
            }
            else
            {
                Show("Текущий пароль указан неверно", false);
            }
        }
        catch (Exception ex)
        {
            Show("Ошибка: " + ex.Message, false);
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}