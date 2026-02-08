using Microsoft.Data.Sqlite;
using MiniTwitns;

var builder = WebApplication.CreateBuilder(args);

// Hardcoded test to query database for first tweet to verify connection works.
MiniTwit miniTwit = new MiniTwit();
miniTwit.Connect_db();
//var param = new SqliteParameter("@Id", 1);
//var res = miniTwit.Query_db("SELECT * FROM message WHERE message_id = @Id", [param]);
var res = miniTwit.Query_db_Read("SELECT * FROM message WHERE message_id < 5", []);
// var res = miniTwit.Get_public_timeline();

foreach (Dictionary<string, object> dict in res) {
  foreach (KeyValuePair<string, object> kvp in dict)
  {
    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
  }
}

/*var msid = new SqliteParameter("@Message_id", SqliteType.Integer) { Value = 11325 };
var auid = new SqliteParameter("@Author_id", SqliteType.Integer) { Value = 1 };
var text = new SqliteParameter("@Text", SqliteType.Text) { Value = "Vi skriver SQL" };
var pub  = new SqliteParameter("@Pub_date", SqliteType.Integer) { Value = 1769777843 };
var flag = new SqliteParameter("@Flagged", SqliteType.Integer) { Value = 0 };

var insert = miniTwit.Query_db_Insert("INSERT INTO message (message_id, author_id, text, pub_date, flagged) VALUES (@Message_id, @Author_id, @Text, @Pub_date, @Flagged)", [msid, auid, text, pub, flag]);

Console.WriteLine(insert);*/

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
    private int PER_PAGE = 30;

    public SqliteConnection Connect_db()
    {
      connection = new SqliteConnection($"Data Source= {DATABASE}");
      connection.Open();
      return connection;
    }

    public List<Dictionary<string, object>> Query_db_Read(string query, SqliteParameter[] args, bool one=false)
    {
      return (List<Dictionary<string, object>>) Query_db(query, args, false, one);
    }

    public int Query_db_Insert(string query, SqliteParameter[] args, bool one=false)
    {
      return (int) Query_db(query, args, true, one);
    }

    public object Query_db(string query, SqliteParameter[] args, bool nonQuery, bool one=false)
    {
      SqliteCommand command = connection.CreateCommand();
      command.CommandText = query;
      foreach (SqliteParameter param in args)
      {
        command.Parameters.Add(param);
      }

      if (nonQuery)
      {
        return command.ExecuteNonQuery();
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

    public List<Dictionary<string, object>> Get_public_timeline()
    {
      string query = """
        Select message.*, user.* 
        from message, user
        where message.flagged = 0 and message.author_id = user.user_id
        order by message.pub_date desc limit @per_page
      """;
      SqliteParameter pp_param = new SqliteParameter("@per_page", PER_PAGE);
      List<Dictionary<string, object>> messages = Query_db_Read(query, [pp_param]);

      return messages;
    } 
  }
}