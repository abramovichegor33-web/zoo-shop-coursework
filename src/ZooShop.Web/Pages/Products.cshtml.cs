using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZooShop.Web.Pages;

public class ProductsModel : PageModel
{
    public User? CurrentUser { get; set; }
    public List<Product> Products { get; set; } = new();
    public List<string> Categories { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string Search { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string Category { get; set; } = "";

    public string Message { get; set; } = "";
    public string Error { get; set; } = "";

    public IActionResult OnGet(string? msg = null, string? err = null)
    {
        CurrentUser = WebSession.GetUser(HttpContext);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Index");
        }

        Message = msg ?? "";
        Error = err ?? "";
        LoadData();
        return Page();
    }

    public IActionResult OnPostAddToCart(int productId, string search = "", string category = "")
    {
        var user = WebSession.GetUser(HttpContext);
        if (user == null) return RedirectToPage("/Index");

        var product = Database.GetProducts().FirstOrDefault(p => p.Id == productId);
        if (product == null || product.Quantity <= 0)
        {
            return RedirectToPage(new { Search = search, Category = category, err = "Товара нет в наличии" });
        }

        var cart = WebSession.GetCart(HttpContext);
        var existing = cart.FirstOrDefault(c => c.ProductId == productId);
        if (existing != null)
        {
            if (existing.Quantity < product.Quantity)
            {
                existing.Quantity++;
                existing.MaxQuantity = product.Quantity;
            }
            else
            {
                return RedirectToPage(new { Search = search, Category = category, err = "В корзине уже максимальное количество со склада" });
            }
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1,
                MaxQuantity = product.Quantity
            });
        }

        WebSession.SetCart(HttpContext, cart);
        return RedirectToPage(new { Search = search, Category = category, msg = $"«{product.Name}» добавлен в корзину" });
    }

    public IActionResult OnPostAddProduct(string name, string categoryName, decimal price, int quantity, string description)
    {
        var user = WebSession.GetUser(HttpContext);
        if (user == null || !user.IsAdmin) return RedirectToPage("/Index");

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(categoryName) || price <= 0 || quantity < 0)
        {
            return RedirectToPage(new { err = "Проверьте правильность заполнения полей товара" });
        }

        Database.AddProduct(name.Trim(), categoryName, price, quantity, description?.Trim() ?? "");
        return RedirectToPage(new { msg = "Товар успешно добавлен" });
    }

    public IActionResult OnPostDeleteProduct(int id)
    {
        var user = WebSession.GetUser(HttpContext);
        if (user == null || !user.IsAdmin) return RedirectToPage("/Index");

        try
        {
            Database.DeleteProduct(id);
            return RedirectToPage(new { msg = "Товар удалён" });
        }
        catch (Exception ex)
        {
            return RedirectToPage(new { err = "Не удалось удалить товар: " + ex.Message });
        }
    }

    private void LoadData()
    {
        Categories = Database.GetCategories();
        var all = Database.GetProducts();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            all = all.Where(p => p.Name.Contains(Search.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(Category) && Category != "Все")
        {
            all = all.Where(p => p.Category == Category).ToList();
        }

        Products = all;
    }
}
