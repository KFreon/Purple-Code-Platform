using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurpleCodePlatform.Auth;
using User = PurpleCodePlatform.Auth.User;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", true);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var azureConfig = builder.Configuration.GetSection(AzureConfig.SectionName).Get<AzureConfig>();
ArgumentNullException.ThrowIfNull(azureConfig, nameof(azureConfig));

var useLocalAuth = builder.Environment.IsDevelopment();
var defaultPolicyName = useLocalAuth ? LocalAuthOptions.SchemeName : OpenIdConnectDefaults.AuthenticationScheme;
var authBuilder = builder.Services.AddAuthentication(defaultPolicyName);

authBuilder
    .AddCookie()
    .AddScheme<LocalAuthOptions, LocalAuthHandler>(LocalAuthOptions.SchemeName, opts => { });

if (!useLocalAuth)
{
    authBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, x =>
    {
        x.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        x.ClientId = azureConfig.ClientId;
        x.ClientSecret = azureConfig.ClientSecret;
        x.Authority = $"https://login.microsoftonline.com/{azureConfig.TenantId}";

        x.Events.OnMessageReceived = context =>
        {
            return Task.CompletedTask;
        };

        x.Events.OnTokenResponseReceived = context =>
        {
            return Task.CompletedTask;
        };

        x.Events.OnAuthorizationCodeReceived = context =>
        {
            return Task.CompletedTask;
        };

        x.Events.OnTokenValidated = async context =>
        {
            // Save user
            var cosmos = context.HttpContext.RequestServices.GetRequiredService<CosmosDbService>();
            var userEmail = context.Principal.Identity.Name;
            var existingUserResult = await cosmos.Get<User>(userEmail, CosmosDbService.UserInfo);
            if (existingUserResult.IsNotFound)
            {
                await cosmos.Upsert<User>(new User { Email = userEmail, Id = userEmail }, CosmosDbService.UserInfo);
            }
        };

        x.Events.OnAuthenticationFailed = context =>
        {
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddAuthorization(config =>
{
    var defaultPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    config.AddPolicy(defaultPolicyName, defaultPolicy);

    config.DefaultPolicy = defaultPolicy;
    config.FallbackPolicy = defaultPolicy;
});

builder.Services.AddTransient(_ =>
{
    var endpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT");
    var key = Environment.GetEnvironmentVariable("COSMOS_KEY");
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    ArgumentNullException.ThrowIfNull(endpoint, nameof(endpoint));
    ArgumentNullException.ThrowIfNull(key, nameof(key));
    ArgumentNullException.ThrowIfNull(env, nameof(env));
    return new CosmosDbService(endpoint, key, env);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseFileServer();

app.MapGet("api/snippets", async ([FromServices] CosmosDbService cosmos) =>
{
    var snippets = await cosmos.GetAll<Snippet>(CosmosDbService.SnippetInfo);
    if (!snippets.Success)
        return Results.Problem(snippets.Error);

    return Results.Ok(snippets.Value);
}).RequireAuthorization();

app.MapPost("api/snippets", async (Snippet snippet, [FromServices] CosmosDbService cosmos, HttpContext context) =>
{
    var userEmail = context.User.Identity.Name;
    snippet.Email = userEmail;
    await cosmos.Upsert(snippet, CosmosDbService.SnippetInfo);
    return Results.Ok();
}).RequireAuthorization();

app.MapDelete("api/snippets/{language}/{id}", async (string language, string id, [FromServices] CosmosDbService cosmos) =>
{
    await cosmos.Delete<Snippet>(id, language, CosmosDbService.SnippetInfo);
    return Results.Ok();
}).RequireAuthorization();

app.Run();


// TODO: Lol date only?
// Not quite working in Net 7, pretty sure it's fixed in 8.
public record Snippet(string Id, string Title, string Code, string LanguageId, int Upvotes, string CreatedOn, string ModifiedOn)
{
    public string Email { get; set; }
}  //TODO: Add comments?


public record AzureConfig(string ClientId, string ClientSecret, string TenantId)
{
    public const string SectionName = "AzureAd";
}