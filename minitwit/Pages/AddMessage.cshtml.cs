using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class AddMessageModel(MiniTwit minitwit) : PageModel
{

    private readonly MiniTwit minitwit = minitwit;
    [BindProperty]
    public required string Text {get;set;}

    public async Task<IActionResult> OnPostAsync()
    {
        //Get current user and ensure it is not null
        string? username = HttpContext.Session.GetString("Logged_In_Username");

        if(username == null)
        {
            TempData["Flash"] = "You must be logged in to post a message.";
            return RedirectToPage("Index");
        }

        bool success = await minitwit.Add_Message(username, Text);
        if (success)
        {
            Console.WriteLine("Message '" + Text + "' from " + username + " added to database!");
            TempData["Flash"] = "Your message was recorded";
        }
        else
        {
            TempData["Flash"] = "Your message was recorded";
        }
        return RedirectToPage("Index");
    }
}
