using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo.Model
{
	public class Meta
	{
		[JsonProperty("src_domain")]
		public string SrcDomain { get; set; }

		[JsonProperty("src_name")]
		public string SrcName { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("dev_milestone")]
		public string DevMilestone { get; set; }

		[JsonProperty("developer")]
		public IList<Developer> Developer { get; set; }

		[JsonProperty("created_date")]
		public object CreatedDate { get; set; }

		[JsonProperty("production_state")]
		public string ProductionState { get; set; }

		[JsonProperty("example_query")]
		public string ExampleQuery { get; set; }

		[JsonProperty("attribution")]
		public object Attribution { get; set; }

		[JsonProperty("designer")]
		public object Designer { get; set; }

		[JsonProperty("src_id")]
		public int? SrcId { get; set; }

		[JsonProperty("src_options")]
		public SrcOptions SrcOptions { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("dev_date")]
		public object DevDate { get; set; }

		[JsonProperty("topic")]
		public IList<string> Topic { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("perl_module")]
		public string PerlModule { get; set; }

		[JsonProperty("src_url")]
		public object SrcUrl { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("live_date")]
		public object LiveDate { get; set; }

		[JsonProperty("producer")]
		public object Producer { get; set; }

		[JsonProperty("is_stackexchange")]
		public object IsStackexchange { get; set; }

		[JsonProperty("signal_from")]
		public string SignalFrom { get; set; }

		[JsonProperty("js_callback_name")]
		public string JsCallbackName { get; set; }

		[JsonProperty("unsafe")]
		public int? Unsafe { get; set; }

		[JsonProperty("blockgroup")]
		public object Blockgroup { get; set; }

		[JsonProperty("maintainer")]
		public Maintainer Maintainer { get; set; }

		[JsonProperty("repo")]
		public string Repo { get; set; }

		[JsonProperty("tab")]
		public string Tab { get; set; }
	}
}