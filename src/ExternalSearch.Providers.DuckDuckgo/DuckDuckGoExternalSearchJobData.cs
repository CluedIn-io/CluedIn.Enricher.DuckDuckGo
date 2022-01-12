using System;
using System.Collections.Generic;
using System.Text;
using CluedIn.Core.Crawling;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo
{
    public class DuckDuckGoExternalSearchJobData : CrawlJobData
    {
        public DuckDuckGoExternalSearchJobData(IDictionary<string, object> configuration)
        {
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>();
        }
    }
}
