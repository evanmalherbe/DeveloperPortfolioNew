using DeveloperPortfolioNew.Models;
using DeveloperPortfolioNew.TransferObjects;
using DeveloperPortfolioNew.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace DeveloperPortfolioNew.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		//private readonly string _apiUrl = "https://localhost:7113"; // local url
		private readonly string _apiUrl;
		private readonly string _getFrameworks = "framework";
		private readonly string _getAboutData = "about";
		private readonly string _getProjectData = "projects";
		private readonly string _postContactData = "contact";

		public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_logger = logger;
			_httpClientFactory = httpClientFactory;
			_apiUrl = configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("API Base URL not found.");
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			// Fetch list of frameworks
			HttpClient client = _httpClientFactory.CreateClient("ApiWithRetries");
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

		[HttpGet]
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

		[HttpGet]
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
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Contact(ContactViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// Fetch education and work experience content
			HttpClient client = _httpClientFactory.CreateClient();
			ContactSubmitDTO payload = new ContactSubmitDTO()
			{
				Name = model.Name,
				Email	= model.Email,
				Message = model.Message
			};
			var formContent = new Dictionary<string, string>()
			{
				{"Name", model.Name},
				{"Email", model.Email},
				{"Message", model.Message},
				{"hp-field", "" }
			};
			var content = new FormUrlEncodedContent(formContent);	
	
			HttpResponseMessage response = new HttpResponseMessage();
			try
			{
				response = await client.PostAsync(_apiUrl + _postContactData, content);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				ViewData["ErrorMessage"] = "API error. Could not submit contact info";
				return View(new ContactViewModel());
			}

			// Failure
			if (response == null || !response.IsSuccessStatusCode)
			{
				ViewData["ErrorMessage"] = "API error. Could not submit contact info";
				return View(new ContactViewModel());
			}

			if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.SeeOther)
			{
			 // redirect to thank you page
				return View("ThankYou");
			}

			// Fallback - return to contact view
			return View(new ContactViewModel());
		}
		public IActionResult ThankYou()
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
