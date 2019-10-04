using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo.Model
{
	public class SearchResult
	{

		[JsonProperty("ImageHeight")]
		public int? ImageHeight { get; set; }

		[JsonProperty("AbstractURL")]
		public string AbstractURL { get; set; }

		[JsonProperty("ImageIsLogo")]
		public int? ImageIsLogo { get; set; }

		[JsonProperty("Results")]
		public List<CoreResult> Results { get; set; }

		[JsonProperty("DefinitionURL")]
		public string DefinitionURL { get; set; }

		[JsonProperty("Redirect")]
		public string Redirect { get; set; }

		[JsonProperty("Answer")]
		public string Answer { get; set; }

		[JsonProperty("Image")]
		public string Image { get; set; }

		[JsonProperty("ImageWidth")]
		public int? ImageWidth { get; set; }

		[JsonProperty("Infobox")]
		public Infobox Infobox { get; set; }

		[JsonProperty("Type")]
		public string Type { get; set; }

		[JsonProperty("Abstract")]
		public string Abstract { get; set; }

		[JsonProperty("DefinitionSource")]
		public string DefinitionSource { get; set; }

		[JsonProperty("Heading")]
		public string Heading { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }

		[JsonProperty("RelatedTopics")]
		public IList<RelatedTopic> RelatedTopics { get; set; }

		[JsonProperty("AbstractText")]
		public string AbstractText { get; set; }

		[JsonProperty("AbstractSource")]
		public string AbstractSource { get; set; }

		[JsonProperty("AnswerType")]
		public string AnswerType { get; set; }

		[JsonProperty("Definition")]
		public string Definition { get; set; }

		[JsonProperty("Entity")]
		public string Entity { get; set; }
	}
}