using System.ComponentModel.DataAnnotations;

namespace DeveloperPortfolioNew.ViewModels
{
	public class AboutViewModel
	{
		public string AboutText { get; set; } = string.Empty;
		public List<Education> Education { get; set; }
		public List<WorkExperience> Work { get; set; }
	}
}
