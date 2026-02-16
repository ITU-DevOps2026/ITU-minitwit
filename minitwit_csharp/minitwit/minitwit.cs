using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.OpenApi;
using minitwit;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromMinutes(15);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});

builder.Services.AddOpenApi();

builder.Services.AddScoped<MiniTwit>();

/* builder.Services.AddControllers().AddJsonOptions(options =>
{
  options.JsonSerializerOptions.WriteIndented = true;
}); */

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

app.UseSession();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();

// This function is necessary because while the script in python can specifically target and run a function
// this does not seem to be achievable in .NET, but this function helps by looking at the argument given to
// when starting the program, ensuring that if init is an argument, we don't actually start the web app, but just
// call init_db() and exit.
if (args.Contains("init"))
{
  var mt = new MiniTwit();
  mt.Init_db();
  // Prevent the program from actually starting
  Environment.Exit(0);
}

app.Run();

namespace minitwit
{
  public class MiniTwit
  {
    // Configuration
    // string DATABASE = "/tmp/minitwit.db";
    private const string Default_Database = "./minitwit.db";
    SqliteConnection? connection;
    private int PER_PAGE = 30;
    private static int _latest = -1;

    // Password hashing configurations
    const int keySize = 32;
    const int iterations = 50000;
    HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;

    public SqliteConnection Connect_db(string db_string = Default_Database)
    {
      connection = new SqliteConnection($"Data Source= {db_string}");
      connection.Open();
      return connection;
    }

    public List<Dictionary<string, object>> Query_db_Read(string query, SqliteParameter[] args, bool one = false)
    {
      return (List<Dictionary<string, object>>)Query_db(query, args, false, one);
    }

    public int Query_db_Insert(string query, SqliteParameter[] args, bool one = false)
    {
      return (int)Query_db(query, args, true, one);
    }

    public object Query_db(string query, SqliteParameter[] args, bool nonQuery, bool one = false)
    {
      if (connection == null) throw new Exception("Connection is null");

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

      reader.Close();

      return results;
    }

    public void Init_db()
    {
      using SqliteConnection connection = Connect_db();
      string schemaSql = File.ReadAllText("schema.sql");
      using SqliteCommand command = connection.CreateCommand();
      command.CommandText = schemaSql;
      command.ExecuteNonQuery();
    }

    public static string Format_datetime(int timestamp)
    {
      return DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().ToString("yyyy-MM-dd @ HH:mm");
    }

    public static string Get_Gravatar_Url(string email, int size = 80)
    {
      byte[] inputBytes = Encoding.UTF8.GetBytes(email.Trim().ToLower());
      byte[] hashBytes = MD5.HashData(inputBytes);
      string hashString = Convert.ToHexString(hashBytes).ToLower();

      return $"https://www.gravatar.com/avatar/{hashString}?d=identicon&s={size}";
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

    public List<Dictionary<string, object>> Get_user_timeline(string username)
    {
      int? profile_user_id = Get_user_id(username);
      if (profile_user_id == null)
      {
        throw new Exception("User doesn't exist");
      }

      string query = """
        select message.*, user.*
        from message, user where
        user.user_id = message.author_id and user.user_id = @user_id
        order by message.pub_date desc limit @per_page
      """;
      SqliteParameter user_id_param = new SqliteParameter("@user_id", SqliteType.Integer)
      {
        Value = profile_user_id
      };

      SqliteParameter pp_param = new SqliteParameter("@per_page", SqliteType.Integer)
      {
        Value = PER_PAGE
      };
      List<Dictionary<string, object>> messages = Query_db_Read(query, [user_id_param, pp_param]);

      return messages;
    }

    public bool Is_following(string active_username, string other_username)
    {
      int? active_user_id = Get_user_id(active_username);
      if (active_user_id == null)
      {
        throw new Exception("Active user doesn't exist");
      }

      int? profile_user_id = Get_user_id(other_username);
      if (profile_user_id == null)
      {
        throw new Exception("User doesn't exist");
      }

      string query = """
        select 1
        from follower 
        where follower.who_id = @active_id and follower.whom_id = @other_id
      """;
      SqliteParameter active_id_param = new SqliteParameter("@active_id", active_user_id);
      SqliteParameter other_id_param = new SqliteParameter("@other_id", profile_user_id);
      List<Dictionary<string, object>> followed = Query_db_Read(query, [active_id_param, other_id_param], true);
      return followed.Count > 0;
    }

    public List<Dictionary<string, object>> Get_my_timeline(string username)
    {
      int? u_ID = Get_user_id(username);

      if (u_ID != null) //Checking that the user exists
      {
        string query = """
          select message.*, user.* from message, user
          where message.flagged = 0 and message.author_id = user.user_id and (
              user.user_id = @user_id or
              user.user_id in (select whom_id from follower
                                      where who_id = @user_id))
          order by message.pub_date desc limit @per_page
        """;
        SqliteParameter user_id_param = new SqliteParameter("@user_id", u_ID);
        SqliteParameter pp_param = new SqliteParameter("@per_page", PER_PAGE);
        List<Dictionary<string, object>> messages = Query_db_Read(query, [user_id_param, pp_param]);

        return messages;
      }
      return new List<Dictionary<string, object>>();
    }

    public void Register(string username, string email, string password)
    {
      string query = """
        INSERT INTO user (username, email, pw_hash) values (@username, @email, @pw_hash)
      """;
      SqliteParameter username_param = new SqliteParameter("@username", username);
      SqliteParameter email_param = new SqliteParameter("@email", email);
      SqliteParameter pw_hash_param = new SqliteParameter("@pw_hash", Generate_password_hash(password));
      Query_db_Insert(query, [username_param, email_param, pw_hash_param]);
    }


    // Code inspired by https://code-maze.com/csharp-hashing-salting-passwords-best-practices/
    public string Generate_password_hash(string password)
    {
      byte[] salt = RandomNumberGenerator.GetBytes(keySize);
      var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, hashAlgorithm, keySize);

      // Format the string to match the format of passwords from python application
      // Saving the salt in the DB is usually bad practice, but we do it because the original application does it :)
      return "pbkdf2:sha256:50000$" + Convert.ToHexString(salt) + "$" + Convert.ToHexString(hash);
    }

    public bool Check_password_hash(string username, string input_password)
    {
      string query = """
        SELECT * FROM user WHERE username = @username
      """;
      SqliteParameter username_param = new SqliteParameter("@username", username);
      var result = Query_db_Read(query, [username_param], true);
      if (result != null && result.Count > 0)
      {
        // Split pw_hash from DB into hashing algorithm, salt, and password hash
        string[] res = ((string)result[0]["pw_hash"]).Split('$');
        var salt_from_DB = Convert.FromHexString(res[1]);
        var pwd_hash_from_DB = res[2];

        // Calculate hash with the input password 
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(input_password), salt_from_DB, iterations, hashAlgorithm, keySize);
        var pwd_hash_from_input = Convert.ToHexString(hash);

        // Compare hash from DB with the hashed input password
        return pwd_hash_from_DB == pwd_hash_from_input;
      }
      return false;
    }

    public int? Get_user_id(string username)
    {
      string query = """
        SELECT user_id FROM user WHERE username = @username
      """;
      SqliteParameter username_param = new SqliteParameter("@username", username);
      var result = Query_db_Read(query, [username_param], true);
      if (result != null && result.Count > 0)
      {
        return Convert.ToInt32(result[0]["user_id"]);
      }
      return null;
    }

    public void Add_Message(string username, string text)
    {
      int? u_ID = Get_user_id(username);

      if (u_ID != null) //Checking that the user exists
      {
        int time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(); //Gets current time
        string query = """
          INSERT INTO message (author_id, text, pub_date, flagged) values (@author_id, @text, @pub_date, @flagged)
        """;
        SqliteParameter author_param = new SqliteParameter("@author_id", u_ID);
        SqliteParameter text_param = new SqliteParameter("@text", text);
        SqliteParameter pub_date_param = new SqliteParameter("@pub_date", time);
        SqliteParameter flag_param = new SqliteParameter("@flagged", SqliteType.Integer) { Value = 0 };
        Query_db_Insert(query, [author_param, text_param, pub_date_param, flag_param]);
      }
    }

    public void Follow_user(string active_username, string username_to_follow)
    {
      int? active_user_id = Get_user_id(active_username);
      if (active_user_id == null)
      {
        throw new Exception("Active user doesn't exist");
      }

      int? profile_user_id = Get_user_id(username_to_follow);
      if (profile_user_id == null)
      {
        throw new Exception("User doesn't exist");
      }

      string query = """
        insert into follower (who_id, whom_id) values (@active_id, @other_id)
      """;
      SqliteParameter active_id_param = new SqliteParameter("@active_id", active_user_id);
      SqliteParameter other_id_param = new SqliteParameter("@other_id", profile_user_id);
      int followed = Query_db_Insert(query, [active_id_param, other_id_param], true);
      if (followed != 1)
      {
        throw new Exception("Something went wrong, when trying to follow");
      }
    }

    public void Unfollow_user(string active_username, string username_to_follow)
    {
      int? active_user_id = Get_user_id(active_username);
      if (active_user_id == null)
      {
        throw new Exception("Active user doesn't exist");
      }

      int? profile_user_id = Get_user_id(username_to_follow);
      if (profile_user_id == null)
      {
        throw new Exception("User doesn't exist");
      }

      string query = """
        delete from follower where who_id=@active_id and whom_id=@other_id
      """;
      SqliteParameter active_id_param = new SqliteParameter("@active_id", active_user_id);
      SqliteParameter other_id_param = new SqliteParameter("@other_id", profile_user_id);
      int unfollowed = Query_db_Insert(query, [active_id_param, other_id_param], true);
      if (unfollowed != 1)
      {
        throw new Exception("Something went wrong, when trying to follow");
      }
    }

    public void UpdateLatest(int? latest)
    {
      if (latest.HasValue)
      {
        _latest = latest.Value;
      }
    }
    public int GetLatest() => _latest;
  }
}