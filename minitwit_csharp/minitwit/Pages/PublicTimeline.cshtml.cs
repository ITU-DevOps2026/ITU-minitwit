using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class PublicTimelineModel : PageModel
{
  public List<Dictionary<string, object>>? Messages { get; private set; }
  public async Task OnGet()
  {
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();

    Messages = await minitwit.Get_public_timeline();
  }
}