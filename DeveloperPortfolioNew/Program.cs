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
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
