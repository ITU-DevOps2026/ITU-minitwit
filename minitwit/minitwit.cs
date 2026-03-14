using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.OpenApi;
using minitwit;
using minitwit.Model;
using Microsoft.EntityFrameworkCore;
using Prometheus;


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

string DbPath = Environment.GetEnvironmentVariable("DbPath") ?? "../data/minitwit.db";

if (args.Contains("init"))
{
  DbPath = "/tmp/minitwit.db";
} 
  
builder.Services.AddDbContext<MinitwitContext>(options =>
  options.UseMySql(DbPath, ServerVersion.AutoDetect(DbPath)));


builder.Services.AddScoped<MiniTwit>();


/* builder.Services.AddControllers().AddJsonOptions(options =>
{
  options.JsonSerializerOptions.WriteIndented = true;
}); */

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<MinitwitContext>();
    context.Database.EnsureCreated();

    //Start the metrics at the amount of tweets and users in the database
    var totalTweets = context.Messages.Count();
    var totalUsers = context.Users.Count();
    
    MinitwitMetrics.TweetCounter.IncTo(totalTweets);
    MinitwitMetrics.UserRegistrationCounter.IncTo(totalUsers);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

// Add Prometheus metrics
app.UseHttpMetrics();
app.MapMetrics();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();

app.Run();

namespace minitwit
{
  public static class MinitwitMetrics
  {
    public static readonly Counter TweetCounter = Metrics
            .CreateCounter("minitwit_tweets_total", "Total number of tweets posted.");

    public static readonly Counter UserRegistrationCounter = Metrics
            .CreateCounter("minitwit_registrations_total", "Total number of user registrations.");
  }
  public class MiniTwit(MinitwitContext minitwitContext)
    {
    private readonly MinitwitContext minitwitContext = minitwitContext; 
    // Configuration
    // string DATABASE = "/tmp/minitwit.db";
    public string DbPath { get; set; } = 
        Environment.GetEnvironmentVariable("DbPath") ?? "../data/minitwit.db";
        
    private const string Default_Database = "../data/minitwit.db";
    private int PER_PAGE = 30;
    private static int _latest = -1;

    // Password hashing configurations
    const int keySize = 32;
    const int iterations = 50000;
    HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;

    public SqliteConnection Connect_db()
    {
      return new SqliteConnection($"Data Source={DbPath};Pooling=False;");
    }

    public async Task<List<Dictionary<string, object>>> Query_db_Read(string query, SqliteParameter[] args, bool one = false)
    {
      return (List<Dictionary<string, object>>) await Query_db(query, args, false, one);
    }

    public async Task<int> Query_db_Insert(string query, SqliteParameter[] args, bool one = false)
    {
      return (int) await Query_db(query, args, true, one);
    }

    public async Task<object> Query_db(string query, SqliteParameter[] args, bool nonQuery, bool one = false)
    {
      using (SqliteConnection connection = Connect_db())
      {
        if (connection == null) throw new Exception("Connection is null");

        await connection.OpenAsync();

        // ADD THIS BLOCK HERE:
        using (var walCommand = connection.CreateCommand())
        {
            walCommand.CommandText = "PRAGMA journal_mode=WAL;";
            await walCommand.ExecuteNonQueryAsync();
        }

        using (SqliteCommand command = connection.CreateCommand())
        {
          command.CommandText = query;
          foreach (SqliteParameter param in args)
          {
            command.Parameters.Add(param);
          }
          if (nonQuery)
          {
            return await command.ExecuteNonQueryAsync();
          }

          using (SqliteDataReader reader = await command.ExecuteReaderAsync())
          {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            while (await reader.ReadAsync())
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
    }

    public async Task Init_db(string db_string = Default_Database)
    {
      using (SqliteConnection connection = new SqliteConnection($"Data Source={db_string};"))
      {
        await connection.OpenAsync();

        // Set WAL mode before running the schema
        using (var walCommand = connection.CreateCommand())
        {
            walCommand.CommandText = "PRAGMA journal_mode=WAL;";
            await walCommand.ExecuteNonQueryAsync();
        }

        string schemaSql = await File.ReadAllTextAsync("schema.sql");
        using (SqliteCommand command = connection.CreateCommand())
        {
          command.CommandText = schemaSql;
          await command.ExecuteNonQueryAsync();
        }
      }
    }

    public static string Format_datetime(int timestamp)
    {
      return DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().ToString("yyyy-MM-dd @ HH:mm");
    }

    public static string Get_Gravatar_Url(string username, int size = 80)
    {
      byte[] inputBytes = Encoding.UTF8.GetBytes(username.Trim().ToLower());
      byte[] hashBytes = MD5.HashData(inputBytes);
      string hashString = Convert.ToHexString(hashBytes).ToLower();

      return $"https://www.gravatar.com/avatar/{hashString}?d=identicon&s={size}";
    }

    public async Task<List<Org.OpenAPITools.Models.Message>> Get_public_timeline()
    {
      var messages = await minitwitContext.Messages
        .Where(m => m.Flagged == 0)
        .OrderByDescending(x => x.PubDate)
        .Take(PER_PAGE)
        .Join(minitwitContext.Users, 
          m => m.AuthorId,
          u => u.UserId, 
          (m, u) => new Org.OpenAPITools.Models.Message
          {
            Content = m.Text,
            User = u.Username,
            PubDate = Format_datetime(m.PubDate ?? 0)
          })
        .ToListAsync();

      return messages;
    }

    public async Task<List<Org.OpenAPITools.Models.Message>> Get_user_timeline(string username)
    {
      int? profile_user_id = await Get_user_id(username);
      if (profile_user_id == null)
      {
        throw new Exception("User doesn't exist");
      }

      List<Org.OpenAPITools.Models.Message> messages = await minitwitContext.Messages
          .Where(m => m.AuthorId == profile_user_id)
          .OrderByDescending(m => m.PubDate)
          .Take(PER_PAGE)
          .Select(
            m => new Org.OpenAPITools.Models.Message
            {
              Content = m.Text,
              User = username,
              PubDate = Format_datetime(m.PubDate ?? 0)
            })
            .ToListAsync();
            
      return messages;
    }

    public async Task<bool> Is_following(string who, string whom)
    {
      int? who_user_id = await Get_user_id(who);
      if (who_user_id == null)
      {
        throw new Exception("Active user doesn't exist");
      }

      int? whom_user_id = await Get_user_id(whom);
      if (whom_user_id == null)
      {
        throw new Exception("User doesn't exist");
      }

      Follower? follows = await minitwitContext.Followers
        .Where(f => f.WhoId == who_user_id && f.WhomId == whom_user_id)
        .FirstOrDefaultAsync(); 
      
      return follows != null;
    }

    public async Task<List<Org.OpenAPITools.Models.Message>> Get_my_timeline(string username)
    {
      int? u_ID = await Get_user_id(username);

      if (u_ID != null) //Checking that the user exists
      {
        List<Org.OpenAPITools.Models.Message> messages = await minitwitContext.Messages
          .Where(m => m.Flagged == 0 && (m.AuthorId == u_ID || minitwitContext.Followers.Any(f => f.WhoId == u_ID && f.WhomId == m.AuthorId)))
          .OrderByDescending(m => m.PubDate)
          .Take(PER_PAGE)
          .Join(minitwitContext.Users, 
            m => m.AuthorId,
            u => u.UserId, 
            (m, u) => new Org.OpenAPITools.Models.Message
            {
              Content = m.Text,
              User = u.Username,
              PubDate = Format_datetime(m.PubDate ?? 0)
            })
          .ToListAsync();

        return messages;
      }
      return new List<Org.OpenAPITools.Models.Message>();
    }

    public async Task Register(string username, string email, string password)
    {
      minitwitContext.Users.Add(new User
      {
        Username = username,
        Email = email,
        PwHash = Generate_password_hash(password)
      });
      await minitwitContext.SaveChangesAsync();

      //Increment the amount of users for the metrics:
      MinitwitMetrics.UserRegistrationCounter.Inc();
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

    public async Task<bool> Check_password_hash(string username, string input_password)
    {
      string? pw_hash = await minitwitContext.Users
        .Where(u => u.Username == username)
        .Select(u => u.PwHash)
        .FirstOrDefaultAsync();
      if (pw_hash != null)
      {
        // Split pw_hash from DB into hashing algorithm, salt, and password hash
        string[] res = pw_hash.Split('$');
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

    public async Task<int?> Get_user_id(string username)
    {

      int user_id = await minitwitContext.Users
        .Where(u => u.Username == username)
        .Select(u => u.UserId)
        .FirstOrDefaultAsync();
      
      return user_id == 0 ? null : user_id;
    }

    public async Task Add_Message(string username, string text)
    {
      int? u_ID = await Get_user_id(username);

      if (u_ID != null) //Checking that the user exists
      {
        int time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(); //Gets current time
        minitwitContext.Messages.Add(new Message
        {
          AuthorId = (int)u_ID,
          Text = text,
          PubDate = time,
          Flagged = 0
        });
        await minitwitContext.SaveChangesAsync();

        //Increment the amount of users for the metrics:
        MinitwitMetrics.TweetCounter.Inc();
      }
    }

    public async Task Follow_user(string who, string whom)
    {
      int? who_user_id = await Get_user_id(who);
      if (who_user_id == null)
      {
        throw new Exception("Active user doesn't exist");
      }

      int? whom_user_id = await Get_user_id(whom);
      if (whom_user_id == null)
      {
        throw new Exception("User doesn't exist");
      }

      bool already_following = await minitwitContext.Followers
        .AnyAsync(f => f.WhoId == who_user_id && f.WhomId == whom_user_id);
      
      if (!already_following)
      {
        minitwitContext.Followers.Add(new Follower
        {
          WhoId = who_user_id,
          WhomId = whom_user_id
        });
        await minitwitContext.SaveChangesAsync();
      }
    }

    public async Task Unfollow_user(string who, string whom)
    {
      int? who_user_id = await Get_user_id(who);
      if (who_user_id == null)
      {
        throw new Exception("Active user doesn't exist");
      }

      int? whom_user_id = await Get_user_id(whom);
      if (whom_user_id == null)
      {
        throw new Exception("User doesn't exist");
      }

      await minitwitContext.Followers
        .Where(f => f.WhoId == who_user_id && f.WhomId == whom_user_id)
        .ExecuteDeleteAsync();
    }

    public void UpdateLatest(int? latest)
    {
      if (latest.HasValue)
      {
        _latest = latest.Value;
      }
    }
    public int GetLatest() => _latest;

    public async Task<Org.OpenAPITools.Models.FollowsResponse> Get_followed_users(string active_username, int? limit)
    {
      int? active_user_id = await Get_user_id(active_username);
      if (active_user_id == null)
      {
        throw new Exception("Active user doesn't exist");
      }

      List<string> followed_users = await minitwitContext.Users
        .Where(u => minitwitContext.Followers.Any(f => f.WhoId == active_user_id && f.WhomId == u.UserId))
        .Take(limit ?? PER_PAGE)
        .Select(u => u.Username)
        .ToListAsync();
      
      Org.OpenAPITools.Models.FollowsResponse response = new Org.OpenAPITools.Models.FollowsResponse
      {
        Follows = followed_users
      };
      return response;
    }
  }
}
