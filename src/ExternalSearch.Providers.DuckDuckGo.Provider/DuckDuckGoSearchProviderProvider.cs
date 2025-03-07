using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Crawling;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.Providers;
using CluedIn.Core.Webhooks;
using CluedIn.ExternalSearch.Providers.DuckDuckgo;
using CluedIn.ExternalSearch.Providers.DuckDuckGo.Vocabularies;
using CluedIn.Providers.Models;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo.Provider
{
    public class DuckDuckGoSearchProviderProvider : ProviderBase, IExtendedProviderMetadata, IExternalSearchProviderProvider
    {
        public IExternalSearchProvider ExternalSearchProvider { get; }

        public DuckDuckGoSearchProviderProvider(ApplicationContext appContext) : base(appContext, GetMetaData())
        {
            ExternalSearchProvider = appContext.Container.ResolveAll<IExternalSearchProvider>().Single(n => n.Id == DuckDuckGoConstants.ProviderId);
        }

        private static IProviderMetadata GetMetaData()
        {
            return new ProviderMetadata
            {
                Id = DuckDuckGoConstants.ProviderId,
                Name = DuckDuckGoConstants.ProviderName,
                ComponentName = DuckDuckGoConstants.ComponentName,
                AuthTypes = new List<string>(),
                SupportsConfiguration = true,
                SupportsAutomaticWebhookCreation = false,
                SupportsWebHooks = false,
                Type = "Enricher"
            };
        }

        public override async Task<CrawlJobData> GetCrawlJobData(ProviderUpdateContext context, IDictionary<string, object> configuration, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var result = new DuckDuckGoExternalSearchJobData(configuration);

            return await Task.FromResult(result);
        }

        public override Task<bool> TestAuthentication(ProviderUpdateContext context, IDictionary<string, object> configuration, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            return Task.FromResult(true);
        }

        public override Task<ExpectedStatistics> FetchUnSyncedEntityStatistics(ExecutionContext context, IDictionary<string, object> configuration, Guid organizationId,
            Guid userId, Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override async Task<IDictionary<string, object>> GetHelperConfiguration(ProviderUpdateContext context, CrawlJobData jobData, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            if (jobData is DuckDuckGoExternalSearchJobData result)
            {
                return await Task.FromResult(result.ToDictionary());
            }

            throw new InvalidOperationException(
                $"Unexpected data type for AcceptanceExternalSearchJobData, {jobData.GetType()}");
        }

        public override Task<IDictionary<string, object>> GetHelperConfiguration(ProviderUpdateContext context, CrawlJobData jobData, Guid organizationId, Guid userId,
            Guid providerDefinitionId, string folderId)
        {
            return GetHelperConfiguration(context, jobData, organizationId, userId, providerDefinitionId);
        }

        public override Task<AccountInformation> GetAccountInformation(ExecutionContext context, CrawlJobData jobData, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            return Task.FromResult(new AccountInformation(providerDefinitionId.ToString(), providerDefinitionId.ToString()));
        }

        public override string Schedule(DateTimeOffset relativeDateTime, bool webHooksEnabled)
        {
            return $"{relativeDateTime.Minute} 0/23 * * *";
        }

        public override Task<IEnumerable<WebHookSignature>> CreateWebHook(ExecutionContext context, CrawlJobData jobData, IWebhookDefinition webhookDefinition,
            IDictionary<string, object> config)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<WebhookDefinition>> GetWebHooks(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteWebHook(ExecutionContext context, CrawlJobData jobData, IWebhookDefinition webhookDefinition)
        {
            throw new NotImplementedException();
        }

        public override Task<CrawlLimit> GetRemainingApiAllowance(ExecutionContext context, CrawlJobData jobData, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            if (jobData == null) throw new ArgumentNullException(nameof(jobData));
            return Task.FromResult(new CrawlLimit(-1, TimeSpan.Zero));
        }

        public override IEnumerable<string> WebhookManagementEndpoints(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public string Icon { get; } = DuckDuckGoConstants.Icon;
        public string Domain { get; } = DuckDuckGoConstants.Domain;
        public string About { get; } = DuckDuckGoConstants.About;
        public AuthMethods AuthMethods { get; } = DuckDuckGoConstants.AuthMethods;
        public IEnumerable<Control> Properties { get; } = DuckDuckGoConstants.Properties;
        public Guide Guide { get; } = DuckDuckGoConstants.Guide;
        public new IntegrationType Type { get; } = DuckDuckGoConstants.IntegrationType;
        public bool SupportsEnricherV2 => true;
        public Dictionary<string, object> ExtraInfo { get; } = new Dictionary<string, object>
        {
            { "autoMap", true },
            { "origin", DuckDuckGoConstants.ProviderName.ToCamelCase() },
            { "originField", string.Empty },
            { "nameKeyField", DuckDuckGoConstants.KeyName.OrgNameKey },
            { "vocabKeyPrefix", DuckDuckGoVocabulary.Organization.KeyPrefix },
            { "autoSubmission", false },
            { "dataSourceSetId", string.Empty },
        };
    }
}
