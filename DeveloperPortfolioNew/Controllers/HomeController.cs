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
		//private readonly string _apiUrl = "https://localhost:7113/api/home"; // local url
		private readonly string _apiUrl = "https://generalapi-7wfz.onrender.com/api/home"; //live url
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
				TechnologyIcons.Add(new TechIcon() 
					{ Name = iconInfo.Name, 
						ClassOrPath = iconInfo.IconClassPath, 
						IconType = iconInfo.IconType,
						BackgroundColour = iconInfo.BackgroundColour
					});
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
			List<ProjectDTO>? projectData = new List<ProjectDTO>();
			try
			{
				projectData = await response.Content.ReadFromJsonAsync<List<ProjectDTO>>();
			}
			catch (Exception ex)
			{
				ViewData["ErrorMessage"] = "Could not load data.";
				return View(new ProjectsViewModel());
			}

			if (projectData == null || !projectData.Any())
			{
				ViewData["ErrorMessage"] = "Could not load data.";
				return View(new ProjectsViewModel());
			}

			// Convert original dto to new one so we can change "technologies" from comma separated strings of technologies into List<string>
			List<ProjectDTO2> list = new List<ProjectDTO2>();

			foreach(ProjectDTO project in projectData)
			{
				// skip over if project is null
				if (project == null) continue;

				ProjectDTO2 dto = new ProjectDTO2();
				if (project?.Technologies != null)
				{
					List<string> techList = project.Technologies.Split(',').ToList();
					dto.Technologies = techList;
				}
				else
				{
					dto.Technologies = new List<string>();
				}
				dto.ID = project.ID;
				dto.GithubLink = project?.GithubLink;
				dto.LiveLink = project?.LiveLink;
				dto.ImagePath = project?.ImagePath;
				dto.Name = project?.Name;
				dto.Description = project?.Description;

				list.Add(dto);
			}

			ProjectsViewModel model = new ProjectsViewModel()
			{
				Projects = list
			};
			return View(model);
		}

		public IActionResult Contact()
		{

			return View(new ContactViewModel());
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
