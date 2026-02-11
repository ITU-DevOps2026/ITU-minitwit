/*using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class BasicTests 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public BasicTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8", 
            response.Content.Headers.ContentType.ToString());
    }
}*/
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

public class MiniTwitTests : IDisposable
{
    private readonly string _baseUrl = "http://localhost:5035";
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public MiniTwitTests()
    {
        _cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
        _client = new HttpClient(handler);
    }

    public void Dispose() => _client.Dispose();

    // --- Helpers ---

    private async Task<HttpResponseMessage> Register(string username, string password, string? password2 = null, string? email = null)
    {
        var data = new Dictionary<string, string>
        {
            { "username", username },
            { "password", password },
            { "password2", password2 ?? password },
            { "email", email ?? $"{username}@example.com" }
        };
        return await _client.PostAsync($"{_baseUrl}/register", new FormUrlEncodedContent(data));
    }

    private async Task<HttpResponseMessage> Login(string username, string password)
    {
        var data = new Dictionary<string, string>
        {
            { "username", username },
            { "password", password }
        };
        return await _client.PostAsync($"{_baseUrl}/login", new FormUrlEncodedContent(data));
    }

    private async Task<HttpResponseMessage> AddMessage(string text)
    {
        var data = new Dictionary<string, string> { { "text", text } };
        var response = await _client.PostAsync($"{_baseUrl}/add_message", new FormUrlEncodedContent(data));
        
        if (!string.IsNullOrEmpty(text))
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Your message was recorded", content);
        }
        return response;
    }

    // --- Tests ---

    [Fact]
    public async Task TestRegister()
    {
        var r = await Register("user1", "default");
        Assert.Contains("You were successfully registered and can login now", await r.Content.ReadAsStringAsync());

        r = await Register("user1", "default");
        Assert.Contains("The username is already taken", await r.Content.ReadAsStringAsync());

        r = await Register("", "default");
        Assert.Contains("You have to enter a username", await r.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TestLoginLogout()
    {
        await Register("user2", "default");
        var r = await Login("user2", "default");
        Assert.Contains("You were logged in", await r.Content.ReadAsStringAsync());

        var logoutResponse = await _client.GetAsync($"{_baseUrl}/logout");
        Assert.Contains("You were logged out", await logoutResponse.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TestMessageRecording()
    {
        await Register("foo", "default");
        await Login("foo", "default");
        
        await AddMessage("test message 1");
        await AddMessage("<test message 2>");

        var r = await _client.GetAsync($"{_baseUrl}/");
        var content = await r.Content.ReadAsStringAsync();
        
        Assert.Contains("test message 1", content);
        Assert.Contains("&lt;test message 2&gt;", content);
    }
}