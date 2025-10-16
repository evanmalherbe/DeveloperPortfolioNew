namespace DeveloperPortfolioNew.Models
{
	public class Project
	{
		public int ID { get; set; }
		public string? Name { get; set; }
		public string? GithubLink { get; set; }
		public string? LiveLink { get; set; }
		public bool IsDisplay { get; set; }
		public int Order { get; set; }
		public string? Description { get; set; }
		public string? Technologies { get; set; }
	}
}
