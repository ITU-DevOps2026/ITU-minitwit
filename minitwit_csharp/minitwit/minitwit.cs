using Microsoft.Data.Sqlite;
using MiniTwitns;

var builder = WebApplication.CreateBuilder(args);

// Hardcoded test to query database for first tweet to verify connection works.
MiniTwit miniTwit = new MiniTwit();
miniTwit.Connect_db();
//var param = new SqliteParameter("@Id", 1);
//var res = miniTwit.Query_db("SELECT * FROM message WHERE message_id = @Id", [param]);
var res = miniTwit.Query_db("SELECT * FROM message WHERE message_id < 5", []);

foreach (Dictionary<string, object> dict in res) {
  foreach (KeyValuePair<string, object> kvp in dict)
  {
      Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
  }
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
    SqliteConnection connection;

    public SqliteConnection Connect_db()
    {
      connection = new SqliteConnection($"Data Source= {DATABASE}");
      connection.Open();
      return connection;
    }

    public List<Dictionary<string, object>> Query_db(string query, SqliteParameter[] args, bool one=false)
    {
      SqliteCommand command = connection.CreateCommand();
      command.CommandText = query;
      foreach (SqliteParameter param in args)
      {
        command.Parameters.Add(param);
      }

      SqliteDataReader reader = command.ExecuteReader();

      List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

      while (reader.Read())
      {
        Dictionary<string, object> row = new Dictionary<string, object>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
          row.Add(reader.GetName(i), reader.GetValue(i));
        }
        results.Add(row);

        if (one) break;
      }

      return results;
    }
  }
}