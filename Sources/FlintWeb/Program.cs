using Flint;
using FlintWeb.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
	options.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddScoped<IFlintService, FlintService>();

var storageConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
var storageContainerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME");
if (builder.Environment.IsDevelopment())
	builder.Services.AddScoped<IStorageService>((_) => new FileSystemStorageService(storageConnectionString, storageContainerName));
else
	builder.Services.AddScoped<IStorageService>((_) => new AzureStorageService(storageConnectionString, storageContainerName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() == false)
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.MapGet("/hc", () => "OK");

app.Run();
