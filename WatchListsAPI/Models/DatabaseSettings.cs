namespace WatchListsAPI.Models
{
	public class DatabaseSettings
	{
		public string ConnectionString { get; set; } = null!;
		public string DatabaseName { get; set; } = null!;
		public string WatchlistsCollectionName { get; set; } = null!;
	}
}
