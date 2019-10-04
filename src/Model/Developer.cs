using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo.Model
{
	public class Developer
	{

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }
	}
}