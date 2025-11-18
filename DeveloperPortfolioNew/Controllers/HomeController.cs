using DeveloperPortfolioNew.Models;
using DeveloperPortfolioNew.TransferObjects;
using DeveloperPortfolioNew.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeveloperPortfolioNew.Controllers
{
	public class HomeController : Controller
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly string _apiUrl;
		private readonly string _getFrameworks = "framework";
		private readonly string _getAboutData = "about";
		private readonly string _getProjectData = "projects";
		private readonly string _postContactData = "contact";

		public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
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

			// Get recaptcha token from hidden field on form
			string recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();

			if (string.IsNullOrEmpty(recaptchaResponse))
			{
				ModelState.AddModelError(string.Empty, "Please complete reCAPTCHA");
				return View(model);
			}

			// Check recaptcha token
			bool isVerified = await VerifyRecaptchaAsync(recaptchaResponse);

			// Failed reCAPTCHA
			if (!isVerified) 
			{
				ModelState.AddModelError(string.Empty, "Failed reCAPTCHA verification. Please try again.");
				return View(model);
			}

			// Passed reCAPTCHA --> continue
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

			string errorMessage = "An unknown API error occurred.";
			// Failure
			if (response == null || !response.IsSuccessStatusCode)
			{
				string responseContent = await response?.Content.ReadAsStringAsync() ?? "";

				if (responseContent == "{}" || string.IsNullOrEmpty(responseContent)) 
				{
					// if response content is empty
					errorMessage = response?.ReasonPhrase != null ? $"Error: {response?.ReasonPhrase}" : errorMessage;
				}
				else
				{ 
					errorMessage = responseContent;
				}
				
				ViewData["ErrorMessage"] = errorMessage;
				return View(new ContactViewModel());
			}

			// Success
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

		private async Task<bool> VerifyRecaptchaAsync(string response)
		{
			var secret = _configuration["GoogleReCaptcha:SecretKey"];
			var client = _httpClientFactory.CreateClient();
			var url = $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}";
			var result = await client.GetAsync(url);
			result.EnsureSuccessStatusCode();
			var content = await result.Content.ReadAsStringAsync();
			var reCaptchaResponse = JsonSerializer.Deserialize<RecaptchVerifyResponse>(content);

			if (!reCaptchaResponse.Success || reCaptchaResponse.Score < 0.5)
			{
				return false;
			}

			return reCaptchaResponse?.Success ?? false;
		}
		public class RecaptchVerifyResponse
		{
			[JsonPropertyName("success")]
			public bool Success { get; set; }
			[JsonPropertyName("score")]
			public float Score { get; set; }
			[JsonPropertyName("action")]
			public string? Action { get; set; }
			[JsonPropertyName("challenge_ts")]
			public DateTime Challenge_ts { get; set; }
			[JsonPropertyName("hostname")]
			public string? Hostname { get; set; }
			[JsonPropertyName("errorcodes")]
			public List<string>? ErrorCodes { get; set; }
		}
	}
}
