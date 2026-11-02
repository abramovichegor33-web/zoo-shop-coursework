using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZooShop.Web.Pages;

public class CartModel : PageModel
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Sum);
    public string Error { get; set; } = "";

    public IActionResult OnGet(string? err = null)
    {
        if (WebSession.GetUser(HttpContext) == null)
        {
            return RedirectToPage("/Index");
        }

        Error = err ?? "";
        Items = WebSession.GetCart(HttpContext);
        return Page();
    }

    public IActionResult OnPostPlus(int productId)
    {
        var cart = WebSession.GetCart(HttpContext);
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            var dbProduct = Database.GetProducts().FirstOrDefault(p => p.Id == productId);
            int max = dbProduct?.Quantity ?? item.MaxQuantity;
            if (item.Quantity < max)
            {
                item.Quantity++;
                item.MaxQuantity = max;
                WebSession.SetCart(HttpContext, cart);
            }
            else
            {
                return RedirectToPage(new { err = $"На складе больше нет товара «{item.Name}» (максимум {max} шт.)" });
            }
        }
        return RedirectToPage();
    }

    public IActionResult OnPostMinus(int productId)
    {
        var cart = WebSession.GetCart(HttpContext);
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity--;
            if (item.Quantity <= 0)
            {
                cart.Remove(item);
            }
            WebSession.SetCart(HttpContext, cart);
        }
        return RedirectToPage();
    }

    public IActionResult OnPostRemove(int productId)
    {
        var cart = WebSession.GetCart(HttpContext);
        cart.RemoveAll(i => i.ProductId == productId);
        WebSession.SetCart(HttpContext, cart);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckoutAsync(
        string paymentMethod,
        string? cardNumber,
        string? cardExpiry,
        string? cardCvv)
    {
        var user = WebSession.GetUser(HttpContext);
        if (user == null) return RedirectToPage("/Index");

        var cart = WebSession.GetCart(HttpContext);
        if (cart.Count == 0)
        {
            return RedirectToPage(new { err = "Корзина пуста" });
        }

        if (paymentMethod == "Банковская карта")
        {
            var digits = Regex.Replace(cardNumber ?? "", @"\D", "");
            if (digits.Length != 16)
            {
                return RedirectToPage(new { err = "Введите корректный 16-значный номер карты" });
            }

            if (string.IsNullOrWhiteSpace(cardExpiry) || !Regex.IsMatch(cardExpiry.Trim(), @"^(0[1-9]|1[0-2])\/\d{2}$"))
            {
                return RedirectToPage(new { err = "Введите срок действия карты в формате ММ/ГГ (например, 12/27)" });
            }

            var cvvDigits = Regex.Replace(cardCvv ?? "", @"\D", "");
            if (cvvDigits.Length != 3)
            {
                return RedirectToPage(new { err = "Введите 3 цифры CVV-кода" });
            }
        }

        await Task.Delay(2000);

        try
        {
            int orderId = Database.PlaceOrder(user.Id, cart, paymentMethod);
            WebSession.SetCart(HttpContext, new List<CartItem>());
            return RedirectToPage("/Orders", new { msg = $"Заказ №{orderId} успешно оплачен и оформлен!" });
        }
        catch (Exception ex)
        {
            return RedirectToPage(new { err = ex.Message });
        }
    }
}
