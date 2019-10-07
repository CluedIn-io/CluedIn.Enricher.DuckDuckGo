using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo.Model
{
	public class CoreMeta
	{

		[JsonProperty("value")]
		public string Value { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }

		[JsonProperty("data_type")]
		public string DataType { get; set; }
	}
}