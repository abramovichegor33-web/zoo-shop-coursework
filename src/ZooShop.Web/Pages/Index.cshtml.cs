using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZooShop.Web.Pages;

public class IndexModel : PageModel
{
    [BindProperty]
    public string Login { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    public string ErrorMessage { get; set; } = "";

    public IActionResult OnGet()
    {
        if (WebSession.GetUser(HttpContext) != null)
        {
            return RedirectToPage("/Products");
        }
        return Page();
    }

    public IActionResult OnPostLogin()
    {
        var user = Database.Login(Login?.Trim() ?? "", Password ?? "");
        if (user == null)
        {
            ErrorMessage = "Неверный логин или пароль";
            return Page();
        }

        WebSession.SetUser(HttpContext, user);
        return RedirectToPage("/Products");
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }

    public IActionResult OnPostColor(string hex, string returnUrl = "/Index")
    {
        WebSession.SetThemeColor(HttpContext, hex);
        return LocalRedirect(returnUrl);
    }
}
