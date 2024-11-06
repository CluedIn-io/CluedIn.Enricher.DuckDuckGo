using System;
using System.Collections.Generic;
using System.Text;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo
{
    public static class DuckDuckGoConstants
    {
        public const string ComponentName = "DuckDuckGo";
        public const string ProviderName = "Duck Duck Go";
        public static readonly Guid ProviderId = Guid.Parse("C7DDBEA4-D5A2-4F25-B2A0-EBFD36D2E8D6");

        public struct KeyName
        {
            public const string AcceptedEntityType = "acceptedEntityType";
            public const string OrgNameKey = "orgNameKey";
            public const string WebsiteKey = "websiteKey";
            public const string CreateEntityCode = "addEntityCode";
        }

        public static string About { get; set; } = "Duck Duck Go is a search engine";
        public static string Icon { get; set; } = "Resources.duckduckgo.svg";
        public static string Domain { get; set; } = "N/A";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods 
        { 
            token = new List<Control>()
            {
                new Control()
                {
                    displayName = "Accepted Entity Type",
                    type = "input",
                    isRequired = true,
                    name = KeyName.AcceptedEntityType
                },
                new Control()
                {
                    displayName = "Organization Name vocab key",
                    type = "input",
                    isRequired = false,
                    name = KeyName.OrgNameKey
                },
                new Control()
                {
                    displayName = "Website vocab key",
                    type = "input",
                    isRequired = false,
                    name = KeyName.WebsiteKey
                },
                new Control()
                {
                    displayName = "Create Entity Code",
                    type = "checkbox",
                    isRequired = false,
                    name = KeyName.CreateEntityCode,
                }
            }
        };

        public static IEnumerable<Control> Properties { get; set; } = new List<Control>()
        {
            // NOTE: Leaving this commented as an example - BF
            //new()
            //{
            //    displayName = "Some Data",
            //    type = "input",
            //    isRequired = true,
            //    name = "someData"
            //}
        };

        public static Guide Guide { get; set; } = null;
        public static IntegrationType IntegrationType { get; set; } = IntegrationType.Enrichment;
    }
}
