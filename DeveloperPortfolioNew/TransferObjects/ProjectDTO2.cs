namespace DeveloperPortfolioNew.TransferObjects
{
	public class ProjectDTO2
	{
				public int ID { get; set; }
		public string? Name { get; set; }
		public string? GithubLink { get; set; }
		public string? LiveLink { get; set; }
		public string? Description { get; set; }
		public List<string> Technologies { get; set; }
		public string? ImagePath { get; set; }
	}
}
