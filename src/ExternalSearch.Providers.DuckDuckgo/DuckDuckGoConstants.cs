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
        public const string Instruction = """
            [
              {
                "type": "bulleted-list",
                "children": [
                  {
                    "type": "list-item",
                    "children": [
                      {
                        "text": "Add the business domain to specify the golden records you want to enrich. Only golden records belonging to that business domain will be enriched."
                      }
                    ]
                  },
                  {
                    "type": "list-item",
                    "children": [
                      {
                        "text": "Add the vocabulary keys to provide the input for the enricher to search for additional information. For example, if you provide the website vocabulary key for the Web enricher, it will use specific websites to look for information about companies. In some cases, vocabulary keys are not required. If you don't add them, the enricher will use default vocabulary keys."
                      }
                    ]
                  }
                ]
              }
            ]
            """;

        public struct KeyName
        {
            public const string AcceptedEntityType = "acceptedEntityType";
            public const string OrgNameKey = "orgNameKey";
            public const string WebsiteKey = "websiteKey";
        }

        public static string About { get; set; } = "Duck Duck Go is a search engine";
        public static string Icon { get; set; } = "Resources.duckduckgo.svg";
        public static string Domain { get; set; } = "N/A";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods
        {
            Token = new List<Control>()
            {
                new Control()
                {
                    DisplayName = "Accepted Business Domain",
                    Type = "entityTypeSelector",
                    IsRequired = true,
                    Name = KeyName.AcceptedEntityType,
                    Help = "The business domain that defines the golden records you want to enrich (e.g., /Organization)."
                },
                new Control()
                {
                    DisplayName = "Name Vocabulary Key",
                    Type = "vocabularyKeySelector",
                    IsRequired = false,
                    Name = KeyName.OrgNameKey,
                    Help = "The vocabulary key that contains the names you want to enrich (e.g., organization.name)."
                },
                new Control()
                {
                    DisplayName = "Website Vocabulary Key",
                    Type = "vocabularyKeySelector",
                    IsRequired = false,
                    Name = KeyName.WebsiteKey,
                    Help = "The vocabulary key that contains the websites you want to enrich (e.g., organization.website)."
                },
            }
        };

        private static Dictionary<string, object> CreateFieldGroup(string id, string label, params string[] fields) => new()
        {
            ["id"] = id,
            ["label"] = label,
            ["fields"] = fields,
        };

        /// <summary>
        /// The sections the UI groups the configuration fields into, in order. Fields no group lists
        /// (such as the filter and rate limit settings added by CluedIn) go to the last group.
        /// </summary>
        public static readonly Dictionary<string, object>[] FieldGroups =
        [
            CreateFieldGroup("whatToEnrich", "What to enrich", KeyName.AcceptedEntityType, "enricherRuleCondition"),
            CreateFieldGroup("fieldMapping", "Field mapping", KeyName.OrgNameKey, KeyName.WebsiteKey),
            CreateFieldGroup("advanced", "Advanced"),
        ];

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

        public static Guide Guide { get; set; } = new Guide
        {
            Instructions = Instruction
        };
        public static IntegrationType IntegrationType { get; set; } = IntegrationType.Enrichment;
    }
}
