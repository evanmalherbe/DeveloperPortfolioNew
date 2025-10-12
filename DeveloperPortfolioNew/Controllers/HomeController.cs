using DeveloperPortfolioNew.Models;
using DeveloperPortfolioNew.TransferObjects;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace DeveloperPortfolioNew.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _apiUrl = "https://localhost:7113/api";
		private readonly string _getFrameworks = "/framework";

		public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_httpClientFactory = httpClientFactory;
		}

		public async Task<IActionResult> Index()
		{
			// Fetch list of frameworks
			HttpClient client = _httpClientFactory.CreateClient();
			HttpResponseMessage response = new HttpResponseMessage();

			try
			{
				response = await client.GetAsync(_apiUrl + _getFrameworks);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			// Fail
			if (response == null || !response.IsSuccessStatusCode) 
			{
				ViewData["ErrorMessage"] = "API error. Could not load data.";
				return View(new List<FrameworkDTO>());
			}

			// Success
			List<FrameworkDTO>? frameworks = new List<FrameworkDTO>();
			try
			{
				frameworks = await response.Content.ReadFromJsonAsync <List<FrameworkDTO>>();
			}
			catch (Exception)
			{
				ViewData["ErrorMessage"] = "Could not load data.";
			}
			
			// Pass list to view
			return View(frameworks);
		}

		public IActionResult About()
		{
			return View();
		}

		public IActionResult Projects()
		{
			return View();
		}

		public IActionResult Contact()
		{
			return View();
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
