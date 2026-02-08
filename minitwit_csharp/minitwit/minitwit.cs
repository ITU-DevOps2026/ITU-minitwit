using Microsoft.Data.Sqlite;
using MiniTwitns;

var builder = WebApplication.CreateBuilder(args);

// Hardcoded test to query database for first tweet to verify connection works.
MiniTwit miniTwit = new MiniTwit();
using SqliteCommand command = miniTwit.connect_db().CreateCommand();
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

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

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

namespace MiniTwitns
{
  class MiniTwit
  {
    // Configuration
    string DATABASE = "./minitwit.db";

    public SqliteConnection connect_db()
    {
      SqliteConnection connection = new SqliteConnection($"Data Source= {DATABASE}");
      connection.Open();
      return connection;
    }
  }
}