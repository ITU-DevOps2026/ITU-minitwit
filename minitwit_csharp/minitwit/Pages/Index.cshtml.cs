using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
  public List<Dictionary<string, object>> Messages { get; private set; }

  public IActionResult OnGet()
  {
    // if logged in show users own timeline
    // if not logged in, redirect to public
    string? username = HttpContext.Session.GetString("Logged_In_Username");
    if(username != null)
    {
      // fetch messages 
      MiniTwit minitwit = new MiniTwit();
      minitwit.Connect_db();

      Messages = minitwit.Get_my_timeline(username);
      return Page();
    }

    return RedirectToPage("./PublicTimeline");
  }
}
