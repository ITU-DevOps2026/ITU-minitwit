using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class LoginModel : PageModel
{
  [BindProperty]
  [Required(ErrorMessage ="You have to enter a username")]
  public required string Username {get; set;}

  [BindProperty]
  [Required(ErrorMessage ="You have to enter a password")]
  public required string Password {get; set;}

  public async Task<IActionResult> OnPostAsync()
  {
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();
    
    if (await minitwit.Get_user_id(Username) == null)
    {
      ModelState.AddModelError("Username", "Invalid username");
    }
    if (!await minitwit.Check_password_hash(Username, Password))
    {
      ModelState.AddModelError("Password", "Invalid password");
    }
    
    if (!ModelState.IsValid)
    {
      return Page();
    }

    // Save the logged in user's username in the browser session 
    HttpContext.Session.SetString("Logged_In_Username", Username);

    TempData["Flash"] = "You were logged in";

    return RedirectToPage("Index");
  }
}