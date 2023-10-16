using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

builder.Services.AddTransient(_ =>
{
    var endpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT");
    var key = Environment.GetEnvironmentVariable("COSMOS_KEY");
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    return new CosmosDbService(endpoint, key, env);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(t => t.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("snippets", async ([FromServices] CosmosDbService cosmos) =>
{
    var snippets = await cosmos.Get<Snippet>();
    return snippets;
});

app.MapPost("snippets", async (Snippet snippet, [FromServices] CosmosDbService cosmos) =>
{
    await cosmos.Upsert(snippet);
    return Results.Ok();
});

app.MapDelete("snippets/{language}/{id}", async (string language, string id, [FromServices] CosmosDbService cosmos) =>
{
    await cosmos.Delete(id, language);
    return Results.Ok();
});

app.Run();


// TODO: Lol date only?
// Not quite working in Net 7, pretty sure it's fixed in 8.
public record Snippet(string Id, string Title, string Code, string LanguageId, int Upvotes, string CreatedOn, string ModifiedOn);  //TODO: Add comments?
