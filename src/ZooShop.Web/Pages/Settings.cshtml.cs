using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZooShop.Web.Pages;

public class SettingsModel : PageModel
{
    public User? CurrentUser { get; set; }
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
        return Page();
    }

    public IActionResult OnPostUpdateName(string fullName)
    {
        var user = WebSession.GetUser(HttpContext);
        if (user == null) return RedirectToPage("/Index");

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return RedirectToPage(new { err = "Имя не может быть пустым" });
        }

        Database.UpdateFullName(user.Id, fullName.Trim());
        user.FullName = fullName.Trim();
        WebSession.SetUser(HttpContext, user);

        return RedirectToPage(new { msg = "Имя успешно обновлено" });
    }

    public IActionResult OnPostChangePassword(string oldPassword, string newPassword)
    {
        var user = WebSession.GetUser(HttpContext);
        if (user == null) return RedirectToPage("/Index");

        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            return RedirectToPage(new { err = "Заполните оба поля пароля" });
        }

        bool ok = Database.ChangePassword(user.Id, oldPassword, newPassword);
        if (!ok)
        {
            return RedirectToPage(new { err = "Старый пароль введён неверно" });
        }

        return RedirectToPage(new { msg = "Пароль успешно изменён" });
    }
}
