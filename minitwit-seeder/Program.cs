using Bogus;
using MySqlConnector;
using System.Text;
using System.Security.Cryptography;

var connectionString = "Server=127.0.0.1;Port=3306;Database=minitwit;Uid=root;Pwd=root;";

Console.WriteLine("🚀 Starting massive seed process (2M+ Tweets)...");
using var conn = new MySqlConnection(connectionString);
await conn.OpenAsync();

// --- PASSWORD CONFIGURATION ---
const int keySize = 32; 
const int iterations = 50000;
HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;

string CreateSeedHash(string password)
{
    byte[] salt = RandomNumberGenerator.GetBytes(keySize);
    var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, hashAlgorithm, keySize);
    return $"pbkdf2:sha256:{iterations}${Convert.ToHexString(salt)}${Convert.ToHexString(hash)}";
}

string commonHash = CreateSeedHash("password123");

// 1. GENERATE 50,000 USERS
Console.WriteLine("👥 Generating 50,000 users...");
var userFaker = new Faker<User>()
    .RuleFor(u => u.username, f => f.Internet.UserName() + f.UniqueIndex)
    .RuleFor(u => u.email, f => f.Internet.Email())
    .RuleFor(u => u.pw_hash, f => commonHash);

var users = userFaker.Generate(50000);
var powerUser = new User { username = "PowerUser", email = "power@user.com", pw_hash = commonHash };
users.Insert(0, powerUser);
await SimpleInsert(users, "user", conn);

var userIds = new List<int>();
using (var cmd = new MySqlCommand("SELECT user_id FROM user", conn))
using (var reader = await cmd.ExecuteReaderAsync())
    while (reader.Read()) userIds.Add(reader.GetInt32(0));

int powerUserId = userIds[0];

// 2. GENERATE MESSAGES (Total target: 2,000,000)
int totalTarget = 2_500_000;
int batchSize = 2500; 
int powerUserTweetCount = 100_000;
int randomTweetCount = totalTarget - powerUserTweetCount;

// A. Power User Tweets
Console.WriteLine($"👑 Generating {powerUserTweetCount} tweets for PowerUser...");
var powerMessages = new Faker<Message>()
    .RuleFor(m => m.author_id, powerUserId)
    .RuleFor(m => m.text, f => f.Rant.Review().Replace("'", "''"))
    .RuleFor(m => m.pub_date, f => (int)new DateTimeOffset(f.Date.Past(1)).ToUnixTimeSeconds())
    .Generate(powerUserTweetCount);

for (int i = 0; i < powerMessages.Count; i += batchSize)
    await SimpleInsert(powerMessages.Skip(i).Take(batchSize), "message", conn);

// B. Random Tweets (The remaining 1.95 Million)
Console.WriteLine($"📨 Generating {randomTweetCount} random tweets...");
var messageFaker = new Faker<Message>()
    .RuleFor(m => m.author_id, f => f.PickRandom(userIds))
    .RuleFor(m => m.text, f => f.Lorem.Sentence().Replace("'", "''"))
    .RuleFor(m => m.pub_date, f => (int)new DateTimeOffset(f.Date.Past(2)).ToUnixTimeSeconds())
    .RuleFor(m => m.flagged, 0);

for (int i = 0; i < (randomTweetCount / batchSize); i++)
{
    var batch = messageFaker.Generate(batchSize);
    await SimpleInsert(batch, "message", conn);
    
    if ((i + 1) % 40 == 0) // Progress update every 100k tweets
    {
        double progress = (double)(i * batchSize) / randomTweetCount * 100;
        Console.WriteLine($"✅ Progress: {progress:F1}% ({(i + 1) * batchSize + powerUserTweetCount} total tweets inserted)");
    }
}

// 3. GENERATE FOLLOWERS (Targeting ~2,000,000 relationships)
Console.WriteLine("📈 Generating realistic social graph (2M follows)...");
var uniquePairs = new HashSet<(int, int)>();

// A. Celebrity follows
var celebrityFollows = userIds.Where(id => id != powerUserId)
    .Select(id => {
        uniquePairs.Add((id, powerUserId));
        return new Follower { who_id = id, whom_id = powerUserId };
    }).ToList();

for (int i = 0; i < celebrityFollows.Count; i += batchSize)
    await SimpleInsert(celebrityFollows.Skip(i).Take(batchSize), "follower", conn);

// B. Random "Noise" follows
int randomFollowTarget = 1_950_000;
Console.WriteLine("🎲 Generating random social connections...");

for (int i = 0; i < randomFollowTarget / batchSize; i++)
{
    var batch = new List<Follower>();
    while (batch.Count < batchSize)
    {
        int who = userIds[Random.Shared.Next(userIds.Count)];
        int whom = userIds[Random.Shared.Next(userIds.Count)];
        
        if (who != whom && uniquePairs.Add((who, whom)))
        {
            batch.Add(new Follower { who_id = who, whom_id = whom });
        }
    }
    await SimpleInsert(batch, "follower", conn);
    if ((i + 1) % 100 == 0) Console.WriteLine($"✅ Follower Progress: {((double)i * batchSize / randomFollowTarget * 100):F1}%");
}

// --- HELPERS ---
async Task SimpleInsert<T>(IEnumerable<T> data, string tableName, MySqlConnection connection)
{
    if (!data.Any()) return;
    var props = typeof(T).GetProperties();
    var columnNames = string.Join(",", props.Select(p => p.Name));
    var sb = new StringBuilder();
    sb.Append($"INSERT INTO {tableName} ({columnNames}) VALUES ");
    var rows = data.Select(item => $"({string.Join(",", props.Select(p => {
        var val = p.GetValue(item);
        return val is string ? $"'{val}'" : val?.ToString() ?? "NULL";
    }))})");
    sb.Append(string.Join(",", rows) + ";");
    using var cmd = new MySqlCommand(sb.ToString(), connection);
    cmd.CommandTimeout = 0; // Disable timeout for big seed batches
    await cmd.ExecuteNonQueryAsync();
}

public class User { public string? username { get; set; } public string? email { get; set; } public string? pw_hash { get; set; } }
public class Message { public int author_id { get; set; } public string? text { get; set; } public int pub_date { get; set; } public int flagged { get; set; } }
public class Follower { public int who_id { get; set; } public int whom_id { get; set; } }