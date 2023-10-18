using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", true);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var azureConfig = builder.Configuration.GetSection(AzureConfig.SectionName).Get<AzureConfig>();
ArgumentNullException.ThrowIfNull(azureConfig, nameof(azureConfig));

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddCookie()
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, x =>
    {
        x.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        x.ClientId = azureConfig.ClientId;
        x.ClientSecret = azureConfig.ClientSecret;
        x.Authority = $"https://login.microsoftonline.com/{azureConfig.TenantId}";

        x.Events.OnAuthenticationFailed = context =>
        {
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(config =>
{
    var defaultPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    config.AddPolicy("AzureAd", defaultPolicy);

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
    // Do "vite watch" and build in container so it's like Prod.

    app.UseSwagger();
    app.UseSwaggerUI();
    //app.UseCors(t => t.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseFileServer();

app.MapGet("snippets",  async  ([FromServices] CosmosDbService cosmos) =>
{
    var snippets = await cosmos.Get<Snippet>();
    return snippets;
}).RequireAuthorization();

app.MapPost("snippets", async (Snippet snippet, [FromServices] CosmosDbService cosmos) =>
{
    await cosmos.Upsert(snippet);
    return Results.Ok();
}).RequireAuthorization(); ;

app.MapDelete("snippets/{language}/{id}",  async (string language, string id, [FromServices] CosmosDbService cosmos) =>
{
    await cosmos.Delete(id, language);
    return Results.Ok();
}).RequireAuthorization();

app.Run();


// TODO: Lol date only?
// Not quite working in Net 7, pretty sure it's fixed in 8.
public record Snippet(string Id, string Title, string Code, string LanguageId, int Upvotes, string CreatedOn, string ModifiedOn);  //TODO: Add comments?


public record AzureConfig(string ClientId, string ClientSecret, string TenantId)
{
    public const string SectionName = "AzureAd";
}