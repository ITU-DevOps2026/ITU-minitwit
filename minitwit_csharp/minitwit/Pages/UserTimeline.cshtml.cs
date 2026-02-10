using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

public class UserTimelineModel : PageModel
{ 
  public List<Dictionary<string, object>> Messages { get; private set; }
  public bool Followed { get; set; }
  public void OnGet(string username)
  {

    // if logged in show users own timeline
    // if not logged in, redirect to public
    string? logged_in_username = HttpContext.Session.GetString("Logged_In_Username");
    
    // fetch messages 
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();

    Messages = minitwit.Get_user_timeline(username);
    if(logged_in_username != null)
    {
      Followed = minitwit.Is_following(logged_in_username, username);
    }
  }
}