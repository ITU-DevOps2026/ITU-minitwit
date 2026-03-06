using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class FollowModel(MiniTwit minitwit) : PageModel
{

  private readonly MiniTwit minitwit = minitwit;
  public async Task<IActionResult> OnGet(string username)
  {
    //Get current user and ensure it is not null
    string? logged_in_username = HttpContext.Session.GetString("Logged_In_Username");
    if(logged_in_username != null)
    {
      await minitwit.Follow_user(logged_in_username, username);
      TempData["Flash"] = $"You are now following \"{username}\"";
      Console.WriteLine(logged_in_username + " now following " + username);
    }

    return RedirectToPage("UserTimeline", new { username });
  }
}