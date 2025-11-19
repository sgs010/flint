using FlintWeb.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
	options.CheckConsentNeeded = context => true;
	options.MinimumSameSitePolicy = SameSiteMode.None;
	options.Secure = CookieSecurePolicy.Always;
});

builder.Services.AddRazorPages(options =>
{
	options.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddScoped<IFlintService, FlintService>();

var app = builder.Build();
app.UseCookiePolicy();
if (app.Environment.IsDevelopment() == false)
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.MapGet("/hc", () => "OK");

app.Run();
