using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZooShop.Web.Pages;

public class OrdersModel : PageModel
{
    public List<OrderRow> Orders { get; set; } = new();
    public string Message { get; set; } = "";

    public IActionResult OnGet(string? msg = null)
    {
        var user = WebSession.GetUser(HttpContext);
        if (user == null)
        {
            return RedirectToPage("/Index");
        }

        Message = msg ?? "";
        Orders = Database.GetOrders(user.Id);
        return Page();
    }
}
