using System.Net;
using System.Net.Http.Json;
using BookTracker.Application.Auth.Login;
using BookTracker.Application.Auth.Register;
using BookTracker.Application.Auth.Shared;
using BookTracker.Application.Books.Create;
using BookTracker.Application.Books.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace BookTracker.Api.Tests;

public sealed class AuthFlowTests : IAsyncLifetime
{
    private const string AuthBase = "/api/v1/auth";
    private const string BooksBase = "/api/v1/books";

    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();
        var cs = _pg.GetConnectionString();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("Database:ConnectionString", cs);
            b.UseSetting("Database:RunMigrationsOnStartup", "true");
            b.UseSetting("Jwt:SigningKey", "test-signing-key-for-integration-test-1234");
            b.UseSetting("Jwt:Issuer", "BookTracker.Test");
            b.UseSetting("Jwt:Audience", "BookTracker.Test");
        });
        _ = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _factory.Dispose();
        await _pg.DisposeAsync();
    }

    [Fact]
    public async Task Register_Login_CreateBook_FlowWorks()
    {
        var client = _factory.CreateClient();
        var email = $"u{Guid.NewGuid():N}@x.com";

        var reg = await client.PostAsJsonAsync($"{AuthBase}/register", new RegisterRequest(email, "Passw0rd"));
        reg.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync($"{AuthBase}/login", new LoginRequest(email, "Passw0rd"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.AccessToken.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);

        var create = await client.PostAsJsonAsync(BooksBase,
            new CreateBookRequest("Clean Code", "Robert C. Martin", 5, "great", DateOnly.FromDateTime(DateTime.UtcNow)));
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<List<BookDto>>(BooksBase);
        list.Should().HaveCount(1);
        list![0].Title.Should().Be("Clean Code");
    }

    [Fact]
    public async Task GetBooks_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync(BooksBase);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InvalidLogin_ReturnsProblemDetails401()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync($"{AuthBase}/login",
            new LoginRequest("missing@example.com", "Wrong@123"));

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("\"traceId\"");
        body.Should().Contain("Invalid credentials.");
    }

    [Fact]
    public async Task ValidationFailure_ReturnsProblemDetails400WithFieldErrors()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync($"{AuthBase}/login",
            new LoginRequest("not-an-email", ""));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("\"errors\"");
    }
}
