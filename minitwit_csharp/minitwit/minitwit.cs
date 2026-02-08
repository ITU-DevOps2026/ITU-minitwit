using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

using SqliteConnection connection = new SqliteConnection("Data Source=./minitwit.db");

connection.Open();

using SqliteCommand command = connection.CreateCommand();
command.CommandText = """
    SELECT text
    FROM message
    WHERE message_id = 1
""";

using var reader = command.ExecuteReader();

while (reader.Read())
{
    Console.WriteLine(reader.GetString(0));
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
