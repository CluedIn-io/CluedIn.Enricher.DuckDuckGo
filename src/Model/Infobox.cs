using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo.Model
{
	public class Infobox
	{

		[JsonProperty("meta")]
		public IList<CoreMeta> Meta { get; set; }

		[JsonProperty("content")]
		public IList<Content> Content { get; set; }
	}
}