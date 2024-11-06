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
            AcceptedEntityType = GetValue<string>(configuration, DuckDuckGoConstants.KeyName.AcceptedEntityType);
            OrgNameKey = GetValue<string>(configuration, DuckDuckGoConstants.KeyName.OrgNameKey);
            WebsiteKey = GetValue<string>(configuration, DuckDuckGoConstants.KeyName.WebsiteKey);
            CreateEntityCodeKey = GetValue<bool?>(configuration, DuckDuckGoConstants.KeyName.CreateEntityCode);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object> {
                { DuckDuckGoConstants.KeyName.AcceptedEntityType, AcceptedEntityType },
                { DuckDuckGoConstants.KeyName.OrgNameKey, OrgNameKey },
                { DuckDuckGoConstants.KeyName.WebsiteKey, WebsiteKey },
                { DuckDuckGoConstants.KeyName.CreateEntityCode, CreateEntityCodeKey },
            };
        }
        public string AcceptedEntityType { get; set; }
        public string OrgNameKey { get; set; }
        public string WebsiteKey { get; set; }
        public bool? CreateEntityCodeKey { get; set; }
    }
}
