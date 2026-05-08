using System.Security.Cryptography;
using System.Text;
using minitwit;
using minitwit.Model;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using System.Reflection;

try
{
  Log.Information("Starting MiniTwit application");

  var builder = WebApplication.CreateBuilder(args);

  builder.Services.AddSerilog((_, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(builder.Configuration)
  );

  builder.Services.AddHealthChecks();

  // Add services to the container.
  builder.Services.AddRazorPages();

  builder.Services.AddSession(options =>
  {
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
  });

  builder.Services.AddOpenApi();

  string? DbPath = Environment.GetEnvironmentVariable("DbPath");

  if (string.IsNullOrEmpty(DbPath))
  {
    builder.Services.AddDbContext<MinitwitContext>(options =>
      options.UseSqlite("DataSource=../data/minitwit.db"));
  }
  else
  {
    builder.Services.AddDbContext<MinitwitContext>(options =>
      options.UseMySql(DbPath, new MySqlServerVersion(new Version(8, 0, 45))));
  }

  builder.Services.AddMetricServer(options => options.Port = 9091);

  builder.Services.AddScoped<MiniTwit>();

  var app = builder.Build();

  using (var scope = app.Services.CreateScope())
  {
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<MinitwitContext>();
    await context.Database.EnsureCreatedAsync();
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

  // Before collecting metrics, we want to update the total amount of tweets and users, so we get the latest values when scraping
  DateTime _lastUpdate = DateTime.MinValue;
  Metrics.DefaultRegistry.AddBeforeCollectCallback(async (cancel) =>
  {
    if ((DateTime.Now - _lastUpdate).TotalSeconds < 30) return; // Skip if too recent
    using (var scope = app.Services.CreateScope()) //The DbContext is a scoped service, so we need to create a scope to get it
    {
      var context = scope.ServiceProvider.GetRequiredService<MinitwitContext>();

      // Query the database for the absolute latest numbers
      var totalTweets = await context.Messages.CountAsync(cancel);
      var totalUsers = await context.Users.CountAsync(cancel);

      // Update the Gauges
      MinitwitMetrics.TotalTweets.Set(totalTweets);
      MinitwitMetrics.TotalUsers.Set(totalUsers);
      _lastUpdate = DateTime.Now;
    }
  });

  app.UseHttpsRedirection();

  app.UseRouting();

  app.UseAuthorization();

  app.UseSession();

  app.MapStaticAssets();
  app.MapRazorPages()
    .WithStaticAssets();

  app.MapControllers();

  app.MapHealthChecks("/healthz");

  await app.RunAsync();
}
catch (Exception ex)
{
  Log.Fatal($"Failed to start {Assembly.GetExecutingAssembly().GetName().Name}", ex);
  throw;
}
finally
{
  await Log.CloseAndFlushAsync();
}

namespace minitwit
{
  public static class MinitwitMetrics
  {
    public static readonly Gauge TotalTweets = Metrics
            .CreateGauge("minitwit_tweets_total", "Total number of tweets in db");

    public static readonly Gauge TotalUsers = Metrics
            .CreateGauge("minitwit_users_total", "Total number of users in db");
  }
  public class MiniTwit(MinitwitContext minitwitContext)
  {
    private readonly MinitwitContext minitwitContext = minitwitContext;
    private readonly int PER_PAGE = 30;

    // Password hashing configurations
    const int keySize = 32;
    const int iterations = 50000;
    readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;
    //tz id which Linux uses for our timezone https://en.wikipedia.org/wiki/List_of_tz_database_time_zones
    private static readonly TimeZoneInfo dkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");

    public static string Format_datetime(int timestamp)
    {
      // Convert timestamp to UTC object
      DateTimeOffset utcTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);

      // Convert from utc to our timezone
      DateTimeOffset dkTime = TimeZoneInfo.ConvertTime(utcTime, dkTimeZone);

      //Formatting
      return dkTime.ToString("yyyy-MM-dd @ HH:mm");
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

      if (u_ID == null) return new List<Org.OpenAPITools.Models.Message>();

      var followedIds = await minitwitContext.Followers
        .Where(f => f.WhoId == u_ID)
        .Select(f => f.WhomId)
        .ToListAsync();

      followedIds.Add(u_ID.Value);

      return await minitwitContext.Messages
        .Where(m => m.Flagged == 0 && followedIds.Contains(m.AuthorId))
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
          }).ToListAsync();
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
      Log.Information("Registered user with name: {username} and email: {email}", username, email);
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

        Log.Information("User {username} posted a tweet, with length: {TextLength}", username, text.Length);
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

    public async Task UpdateLatest(int? latest)
    {
      if (latest.HasValue)
      {
        //Get the latest Database entry, so this can be updated
        var latest_entry = await minitwitContext.LatestInt.FirstOrDefaultAsync(l => l.Id == 1);

        if (latest_entry != null)
        {
          // Overwrite the value
          latest_entry.Value = latest.Value;

          // Persist to database
          await minitwitContext.SaveChangesAsync();
        }
        else
        {
          // Handle the case where the entry doesn't exist yet
          await minitwitContext.LatestInt.AddAsync(new Latest { Id = 1, Value = latest.Value });
          await minitwitContext.SaveChangesAsync();
        }
      }
    }
    public async Task<int> GetLatest()
    {
      var latest_entry = await minitwitContext.LatestInt.FirstOrDefaultAsync(l => l.Id == 1);

      if (latest_entry != null)
      {
        return latest_entry.Value;
      }
      else
      {
        //If we haven't gotten an latestValue yet
        return -1;
      }
    }

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
