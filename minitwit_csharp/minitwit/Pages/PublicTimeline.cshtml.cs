using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit;

namespace minitwit.Pages;

public class PublicTimelineModel : PageModel
{ 
  public List<Dictionary<string, object>> Messages { get; private set; }
  public override void OnGet()
  {
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();

    Messages = minitwit.Get_public_timeline();
  }
}