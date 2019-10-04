using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo.Model
{
	public class Maintainer
	{

		[JsonProperty("github")]
		public string Github { get; set; }
	}
}