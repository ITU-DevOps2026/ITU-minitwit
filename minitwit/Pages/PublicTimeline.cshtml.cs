using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.OpenAPITools.Models;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class PublicTimelineModel(MiniTwit minitwit) : PageModel
{
  private readonly MiniTwit minitwit = minitwit;

  public List<Message>? Messages { get; private set; }
  public async Task OnGet()
  {
    Messages = await minitwit.Get_public_timeline();
  }
}