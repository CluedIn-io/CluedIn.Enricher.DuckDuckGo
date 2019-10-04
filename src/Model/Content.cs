using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo.Model
{
	public class Content
	{

		[JsonProperty("value")]
		public object Value { get; set; }

		[JsonProperty("sort_order")]
		public string SortOrder { get; set; }

		[JsonProperty("data_type")]
		public string DataType { get; set; }

		[JsonProperty("wiki_order")]
		public object WikiOrder { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }
	}
}