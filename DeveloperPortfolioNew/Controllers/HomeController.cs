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
		public struct TechIcon
		{
				public string Name { get; set; }
				public string ClassOrPath { get; set; } // This will hold the FA class OR the SVG file path
				public string IconType { get; set; }    // "FA" or "SVG"
		}

		public static readonly TechIcon[] TechnologyIcons2 = new TechIcon[]
		{
				// Existing Font Awesome Icon
				new TechIcon { Name = "React.JS", ClassOrPath = "fa-brands fa-react", IconType = "FA" }, 
    
				// New SVG Icon (Requires saving the SVG to your project)
				new TechIcon { Name = "MongoDB", ClassOrPath = "/images/icons/mongodb.svg", IconType = "SVG" }, 
    
				// New Dedicated .NET Core Icon (If you find the SVG)
				new TechIcon { Name = ".NET Core", ClassOrPath = "/images/icons/dotnetcore.svg", IconType = "SVG" },
		};
		public static readonly Dictionary<string, string> TechnologyIcons = new Dictionary<string, string>
		{
      // Key: Technology Name, Value: Font Awesome Class

      // Front-End Languages & Frameworks
      { "HTML", "fa-brands fa-html5" },
      { "CSS", "fa-brands fa-css3-alt" },
      { "JavaScript", "fa-brands fa-square-js" },
      { "React.JS", "fa-brands fa-react" },
      { "Bootstrap", "fa-brands fa-bootstrap" },

      // Back-End/Database/APIs
      { "Node.JS", "fa-brands fa-node-js" },
      { "Express", "fa-solid fa-server" },              // Using generic server icon
      { "MongoDB", "fa-solid fa-database" },            // Using generic database icon
      { "Rest APIs", "fa-solid fa-code" },              // Using generic code icon
      { "Microsoft SQL Server", "fa-solid fa-database" },
      { "JWT (JSON Web Tokens)", "fa-solid fa-key" },   // Using key/security icon
      { "JQuery", "fa-solid fa-hand-pointer" },         // Using hand-pointer icon

      // C# and .NET Ecosystem
      { "C#", "fa-solid fa-hashtag" },                  // Using hashtag/sharp symbol
      { ".NET Framework", "fa-solid fa-server" },
      { ".NET Core", "fa-solid fa-terminal" },
      { "Entity Framework Core", "fa-solid fa-database" },

      // Tools and Version Control
      { "Git", "fa-brands fa-git-alt" },
      { "GitHub", "fa-brands fa-github" },
      { "Azure DevOps", "fa-brands fa-microsoft" }      // Using generic Microsoft icon
		};
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
				frameworks = await response.Content.ReadFromJsonAsync<List<FrameworkDTO>>();
			}
			catch (Exception)
			{
				ViewData["ErrorMessage"] = "Could not load data.";
			}

			
			
			// Pass list to view
			return View(frameworks);
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
