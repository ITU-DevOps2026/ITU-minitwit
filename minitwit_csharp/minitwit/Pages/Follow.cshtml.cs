using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class FollowModel : PageModel
{
  /*public async Task<IActionResult> OnPostAsync(string username)
  {
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();

    //Get current user and ensure it is not null
    string? logged_in_username = HttpContext.Session.GetString("Logged_In_Username");
    if(logged_in_username != null)
    {
      minitwit.Follow_user(logged_in_username, username);
      Console.WriteLine(logged_in_username + " now following " + username);
    }

    return RedirectToPage("UserTimeline", new { username });
  }*/

  public IActionResult OnGet(string username)
  {
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();

    //Get current user and ensure it is not null
    string? logged_in_username = HttpContext.Session.GetString("Logged_In_Username");
    if(logged_in_username != null)
    {
      minitwit.Follow_user(logged_in_username, username);
      Console.WriteLine(logged_in_username + " now following " + username);
    }

    return RedirectToPage("UserTimeline", new { username });
  }
}