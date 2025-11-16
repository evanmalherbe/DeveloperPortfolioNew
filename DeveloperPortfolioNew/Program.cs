using Microsoft.Extensions.FileProviders;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add HttpClientFactory service
//builder.Services.AddHttpClient();

// Retry policy in case remote api is asleep or still spinning up
var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(new[]
{
	TimeSpan.FromSeconds(1),
	TimeSpan.FromSeconds(3),
	TimeSpan.FromSeconds(5)

}, (exception, timeSpan, retryCount, context) => 
{
	Console.WriteLine($"API request failed. Retrying in {timeSpan.Seconds}s. Attempt: {retryCount}");
});
// Add HttpClient service and attached the defined retry policy
builder.Services.AddHttpClient("ApiWithRetries").AddPolicyHandler(retryPolicy);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}
else
{
	// if app is in production environment
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

// Define the static file provider to point only to the 'images' subfolder
var imageFileProvider = new PhysicalFileProvider(
	Path.Combine(builder.Environment.WebRootPath, "images")
	);

// Apply separate options for image caching
app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = imageFileProvider,
	RequestPath = "/images",
	OnPrepareResponse = ctx =>
	{
		// Cache images for a full 30 days
		const int durationInSeconds = 60 * 60 * 24 * 30;
		ctx.Context.Response.Headers.Append("Cacht-Control", $"public, max-age={durationInSeconds}");
	}
});

// Use the default static files configuration for everything else (CSS, JS etc)
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
