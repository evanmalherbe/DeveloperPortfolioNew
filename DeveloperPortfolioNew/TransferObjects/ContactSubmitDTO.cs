using System.ComponentModel.DataAnnotations;

namespace DeveloperPortfolioNew.TransferObjects
{
	public class ContactSubmitDTO
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string Message { get; set; }
	}
}
