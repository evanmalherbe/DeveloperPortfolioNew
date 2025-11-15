using System.ComponentModel.DataAnnotations;

namespace DeveloperPortfolioNew.ViewModels
{
	public class ContactViewModel
	{
		[Required(ErrorMessage = "Name is required")]
		[DataType(DataType.Text)]
		[MaxLength(100)]
		public string Name { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email format")]
		[MaxLength(255)]
		public string Email { get; set; }

		[Required(ErrorMessage = "Message is required")]
		[DataType(DataType.Text)]
		[MaxLength(4000)]
		public string Message { get; set; }
	}
}
