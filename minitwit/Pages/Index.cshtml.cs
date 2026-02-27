using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class IndexModel(MiniTwit minitwit) : PageModel
{

  private readonly MiniTwit minitwit = minitwit;
  public List<Dictionary<string, object>>? Messages { get; private set; }

  public async Task<IActionResult> OnGet()
  {
    // if logged in show users own timeline
    // if not logged in, redirect to public
    string? username = HttpContext.Session.GetString("Logged_In_Username");
    if(username != null)
    {
      Messages = await minitwit.Get_my_timeline(username);
      return Page();
    }

    return RedirectToPage("./PublicTimeline");
  }
}
