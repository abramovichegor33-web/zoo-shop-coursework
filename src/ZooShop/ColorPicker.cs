using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace ZooShop;

public static class ThemePicker
{
    public static void Attach(Button button)
    {
        var flyout = new Flyout();
        flyout.Opening += (_, _) => flyout.Content = BuildPanel(flyout);
        button.Flyout = flyout;
    }

    private static Control BuildPanel(Flyout flyout)
    {
        var panel = new WrapPanel { Width = 220 };
        foreach (var item in AppColors.All)
        {
            var name = item.Key;
            var isCurrent = name == AppColors.CurrentName;
            var circle = new Border
            {
                Width = 34,
                Height = 34,
                Margin = new Thickness(4),
                CornerRadius = new CornerRadius(17),
                Background = new SolidColorBrush(Color.Parse(item.Value)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(isCurrent ? 3 : 0),
                Cursor = new Cursor(StandardCursorType.Hand)
            };
            ToolTip.SetTip(circle, name);
            circle.PointerPressed += (_, _) =>
            {
                AppColors.Apply(name);
                flyout.Hide();
            };
            panel.Children.Add(circle);
        }
        return panel;
    }
}