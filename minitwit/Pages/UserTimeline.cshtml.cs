using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

public class UserTimelineModel : PageModel
{ 
  public List<Dictionary<string, object>>? Messages { get; private set; }
  public bool Followed { get; set; }
  public async Task<IActionResult> OnGet(string username)
  {
    string? logged_in_username = HttpContext.Session.GetString("Logged_In_Username");

    // if username is logged in user, then show own timeline.
    /*if (logged_in_username == username)
    {
      return RedirectToPage("./Index");
    }*/

    // fetch messages 
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();

    Messages = await minitwit.Get_user_timeline(username);
    if(logged_in_username != null)
    {
      Followed = await minitwit.Is_following(logged_in_username, username);
    }
    return Page();
  }
}