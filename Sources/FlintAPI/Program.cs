using System.Runtime.CompilerServices;
using Flint.Services;

[assembly: InternalsVisibleTo("FlintTests")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ApiService, ApiService>();
builder.Services.AddScoped<IBlobService, BlobService>();

var app = builder.Build();

app.MapGet("/hc", () => "OK");

app.MapPost("/check", async (IFormFile asm, IFormFile pdb, ApiService api) =>
{
	var result = await api.CheckAsync(asm, pdb);
	return Results.Ok(result);
});

app.Run();
