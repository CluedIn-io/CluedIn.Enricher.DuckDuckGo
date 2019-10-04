using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo.Model
{
	public class SrcOptions
	{

		[JsonProperty("directory")]
		public string Directory { get; set; }

		[JsonProperty("is_mediawiki")]
		public int? IsMediawiki { get; set; }

		[JsonProperty("skip_end")]
		public string SkipEnd { get; set; }

		[JsonProperty("language")]
		public string Language { get; set; }

		[JsonProperty("min_abstract_length")]
		public string MinAbstractLength { get; set; }

		[JsonProperty("skip_image_name")]
		public int? SkipImageName { get; set; }

		[JsonProperty("skip_qr")]
		public string SkipQr { get; set; }

		[JsonProperty("is_fanon")]
		public int? IsFanon { get; set; }

		[JsonProperty("skip_abstract_paren")]
		public int? SkipAbstractParen { get; set; }

		[JsonProperty("skip_abstract")]
		public int? SkipAbstract { get; set; }

		[JsonProperty("skip_icon")]
		public int? SkipIcon { get; set; }

		[JsonProperty("is_wikipedia")]
		public int? IsWikipedia { get; set; }

		[JsonProperty("src_info")]
		public string SrcInfo { get; set; }

		[JsonProperty("source_skip")]
		public string SourceSkip { get; set; }
	}
}