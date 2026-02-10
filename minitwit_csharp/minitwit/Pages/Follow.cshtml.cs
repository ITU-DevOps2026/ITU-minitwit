using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class FollowModel : PageModel
{
  [TempData]
  public string? FollowResult { get; set; }


  public IActionResult OnGet(string username)
  {
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();

    //Get current user and ensure it is not null
    string? logged_in_username = HttpContext.Session.GetString("Logged_In_Username");
    if(logged_in_username != null)
    {
      minitwit.Follow_user(logged_in_username, username);
      FollowResult = $"You are now following \"{username}\"";
      Console.WriteLine(logged_in_username + " now following " + username);
    }

    return RedirectToPage("UserTimeline", new { username });
  }
}