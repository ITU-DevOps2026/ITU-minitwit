using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.OpenAPITools.Models;

namespace minitwit.Pages;

public class UserTimelineModel(MiniTwit minitwit) : PageModel
{

  private readonly MiniTwit minitwit = minitwit;
  public List<Message>? Messages { get; private set; }
  public bool Followed { get; set; }
  public async Task<IActionResult> OnGet(string username)
  {
    try
    {
      string? logged_in_username = HttpContext.Session.GetString("Logged_In_Username");

      Messages = await minitwit.Get_user_timeline(username);
      if(logged_in_username != null)
      {
        Followed = await minitwit.Is_following(logged_in_username, username);
      }
      return Page();
    }
    catch (Exception ex) when (ex.Message.Contains("User doesn't exist"))
    {
        return NotFound("404: User not found");
    }
  }
}