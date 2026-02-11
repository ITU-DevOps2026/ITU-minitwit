using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class LogoutModel : PageModel
{
  public IActionResult OnGet()
  {
    HttpContext.Session.Clear();

    TempData["Flash"] = "You were logged out";

    return RedirectToPage("/PublicTimeline");
  }
}