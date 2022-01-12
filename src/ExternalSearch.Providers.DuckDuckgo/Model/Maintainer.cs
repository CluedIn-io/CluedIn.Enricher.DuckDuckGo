using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo.Model
{
	public class Maintainer
	{

		[JsonProperty("github")]
		public string Github { get; set; }
	}
}