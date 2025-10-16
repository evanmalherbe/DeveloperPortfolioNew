using DeveloperPortfolioNew.Models;
using DeveloperPortfolioNew.TransferObjects;
using DeveloperPortfolioNew.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace DeveloperPortfolioNew.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _apiUrl = "https://localhost:7113/api/home";
		private readonly string _getFrameworks = "/framework";
		private readonly string _getAboutData = "/about";
		private readonly string _getProjectData = "/projects";

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
				response = null;
			}
			// Fail
			if (response == null || !response.IsSuccessStatusCode)
			{
				ViewData["ErrorMessage"] = "API error. Could not load data.";
				return View(new IndexViewModel());
			}

			// Success
			List<FrameworkDTO>? frameworks = new List<FrameworkDTO>();
			try
			{
				frameworks = await response.Content.ReadFromJsonAsync<List<FrameworkDTO>>();
			}
			catch (Exception)
			{
				ViewData["ErrorMessage"] = "Could not load data.";
			}
			List<TechIcon> TechnologyIcons = new List<TechIcon>();

			foreach (FrameworkDTO iconInfo in frameworks)
			{
				TechnologyIcons.Add(new TechIcon() { Name = iconInfo.Name, ClassOrPath = iconInfo.IconClassPath, IconType = iconInfo.IconType });
			}
			IndexViewModel indexViewModel = new IndexViewModel()
			{
				Frameworks = frameworks,
				TechnologyIcons = TechnologyIcons,
			};
			// Pass list to view
			return View(indexViewModel);
		}

		public async Task<IActionResult> About()
		{
			// Fetch education and work experience content
			HttpClient client = _httpClientFactory.CreateClient();
			HttpResponseMessage response = new HttpResponseMessage();

			try
			{
				response = await client.GetAsync(_apiUrl + _getAboutData);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				ViewData["ErrorMessage"] = "API error. Could not load data.";
				return View(new AboutViewModel());
			}
			// Fail
			if (response == null || !response.IsSuccessStatusCode)
			{
				ViewData["ErrorMessage"] = "API error. Could not load data.";
				return View(new AboutViewModel());
			}

			// Success
			AboutViewModel? aboutData = new AboutViewModel();
			try
			{
				aboutData = await response.Content.ReadFromJsonAsync<AboutViewModel>();
			}
			catch (Exception ex)
			{
				ViewData["ErrorMessage"] = "Could not load data.";
			}

			// Pass list to view
			return View(aboutData);
		}

		public async Task<IActionResult> Projects()
		{
			// Fetch education and work experience content
			HttpClient client = _httpClientFactory.CreateClient();
			HttpResponseMessage response = new HttpResponseMessage();

			try
			{
				response = await client.GetAsync(_apiUrl + _getProjectData);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				ViewData["ErrorMessage"] = "API error. Could not load data.";
				return View(new ProjectsViewModel());
			}
			// Fail
			if (response == null || !response.IsSuccessStatusCode)
			{
				ViewData["ErrorMessage"] = "API error. Could not load data.";
				return View(new ProjectsViewModel());
			}

			// Success
			ProjectsViewModel? projectData = new ProjectsViewModel();
			try
			{
				projectData = await response.Content.ReadFromJsonAsync<ProjectsViewModel>();
			}
			catch (Exception ex)
			{
				ViewData["ErrorMessage"] = "Could not load data.";
			}

			return View(projectData);
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
