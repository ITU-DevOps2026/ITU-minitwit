using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit;

namespace minitwit.Pages;

public class TimelineModel : PageModel
{
  public List<Dictionary<string, object>> Messages { get; private set; }
  

  public void OnGet()
  {
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();

    Messages = minitwit.Get_public_timeline();
  }
}