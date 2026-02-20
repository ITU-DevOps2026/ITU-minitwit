using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class RegisterModel : PageModel
{
  [BindProperty]
  [Required(ErrorMessage = "You have to enter a username")]
  public required string Username { get; set; }

  [BindProperty]
  [Required(ErrorMessage = "You have to enter a valid email address")]
  [EmailAddress]
  public required string Email { get; set; }

  [BindProperty]
  [Required(ErrorMessage = "You have to enter a password")]
  public required string Password { get; set; }

  [BindProperty]
  [Required(ErrorMessage = "Please repeat your password")]
  [Compare("Password", ErrorMessage = "The two passwords do not match")]
  public required string Password2 { get; set; }

  public async Task<IActionResult> OnPostAsync()
  {
    MiniTwit minitwit = new MiniTwit();
    minitwit.Connect_db();

    if (Username != null && await minitwit.Get_user_id(Username) != null)
    {
      ModelState.AddModelError("Username", "The username is already taken");
    }

    if (!ModelState.IsValid)
    {
      return Page();
    }

    await minitwit.Register(Username!, Email, Password);

    TempData["Flash"] = "You were successfully registered and can login now";

    return RedirectToPage("./login");
  }
}