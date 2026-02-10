using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace minitwit.Pages;

[IgnoreAntiforgeryToken]
public class AddMessageModel : PageModel
{
    [BindProperty]
    public required string Text {get;set;}

    public async Task<IActionResult> OnPostAsync()
    {
        MiniTwit minitwit = new MiniTwit();
        minitwit.Connect_db();

        //Get current user and ensure it is not null
        string? username = HttpContext.Session.GetString("Logged_In_Username");
        if(username != null)
        {
            minitwit.Add_Message(username, Text);
            Console.WriteLine("Message '" + Text + "' from " + username + " added to database!");
        }

        return RedirectToPage("Index");
    }
}
