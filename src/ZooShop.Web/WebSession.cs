using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ZooShop.Web;

public static class WebSession
{
    public static readonly Dictionary<string, string> Colors = new()
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

    public static User? GetUser(HttpContext ctx)
    {
        var json = ctx.Session.GetString("User");
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<User>(json);
    }

    public static void SetUser(HttpContext ctx, User? user)
    {
        if (user == null)
        {
            ctx.Session.Remove("User");
        }
        else
        {
            ctx.Session.SetString("User", JsonSerializer.Serialize(user));
        }
    }

    public static List<CartItem> GetCart(HttpContext ctx)
    {
        var json = ctx.Session.GetString("Cart");
        return string.IsNullOrEmpty(json) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(json)!;
    }

    public static void SetCart(HttpContext ctx, List<CartItem> cart)
    {
        ctx.Session.SetString("Cart", JsonSerializer.Serialize(cart));
    }

    public static string GetThemeColor(HttpContext ctx)
    {
        var color = ctx.Request.Cookies["ThemeColor"];
        if (!string.IsNullOrEmpty(color) && Colors.ContainsValue(color))
        {
            return color;
        }
        return "#2E7D32";
    }

    public static void SetThemeColor(HttpContext ctx, string hex)
    {
        if (Colors.ContainsValue(hex))
        {
            ctx.Response.Cookies.Append("ThemeColor", hex, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(365)
            });
        }
    }
}
