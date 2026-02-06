# Week 2 06/02 - 13/02

## Choice of tech stack
We've decided to refactor our ITU-MiniTwit from a Python/Flask application, into a C#/RazorPages application. We've chosen C# as it is a language all group members have past experience with, that we want to build further upon especially in the context of DevOps. Furthermore it is a commonly used language in the industry <https://survey.stackoverflow.co/2025/technology#most-popular-technologies-language-prof>, with extensive documentation. 
C# also offers easy integration with .NET, providing several advantages such as cross-platform compatibility, a comprehensive set of libraries, a robust CLI, and a powerful package manager (NuGet): <https://learn.microsoft.com/en-us/dotnet/core/introduction>. While not applicable yet, using .NET also keeps the possibility open of using something like Blazor later on, a frontend framework in .NET, which means we can write our frontend in C# as well. 
We use Razor Pages as it follows a page-based model, which is very similar to what is in the current Flask application, allowing us to reuse our the static artifacts and templates. 
