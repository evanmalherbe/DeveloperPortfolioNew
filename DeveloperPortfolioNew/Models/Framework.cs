namespace DeveloperPortfolioNew.Models
{
	public class Framework
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public bool IsDisplay { get; set; }
		public int Order { get; set; } = 0;
		public string IconClassPath { get; set; }
		public string IconType { get; set; }
	}
}
