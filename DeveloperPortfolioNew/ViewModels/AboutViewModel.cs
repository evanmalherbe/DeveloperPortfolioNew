using System.ComponentModel.DataAnnotations;

namespace DeveloperPortfolioNew.ViewModels
{
	public class AboutViewModel
	{
		public List<Education> Education { get; set; }
		public List<WorkExperience> Work { get; set; }
	}
}
