using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo.Model
{
	public class RelatedTopic
	{

		[JsonProperty("Text")]
		public string Text { get; set; }

		[JsonProperty("Result")]
		public string Result { get; set; }

		[JsonProperty("Icon")]
		public Icon Icon { get; set; }

		[JsonProperty("FirstURL")]
		public string FirstURL { get; set; }
	}
}