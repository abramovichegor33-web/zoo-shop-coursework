using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace ZooShop;

public static class AppColors
{
    public static readonly Dictionary<string, string> All = new()
    {
        { "Зелёный", "#2E7D32" },
        { "Синий", "#1565C0" },
        { "Красный", "#C62828" },
        { "Жёлтый", "#F9A825" },
        { "Оранжевый", "#EF6C00" },
        { "Фиолетовый", "#6A1B9A" },
        { "Розовый", "#D81B60" },
        { "Бирюзовый", "#00838F" },
        { "Коричневый", "#5D4037" },
        { "Серый", "#546E7A" }
    };

    public static List<string> Names => new(All.Keys);
    public static string CurrentName = "Зелёный";

    public static void Apply(string name)
    {
        if (!All.TryGetValue(name, out var hex))
        {
            return;
        }
        CurrentName = name;
        Application.Current!.Resources["AccentBrush"] = new SolidColorBrush(Color.Parse(hex));
    }
}