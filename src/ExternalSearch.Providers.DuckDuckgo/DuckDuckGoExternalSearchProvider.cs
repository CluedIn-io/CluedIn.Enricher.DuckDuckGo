// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoExternalSearchProvider.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the duck go external search provider class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.Providers;
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Filters;
using CluedIn.ExternalSearch.Providers.DuckDuckgo;
using CluedIn.ExternalSearch.Providers.DuckDuckGo.Model;
using CluedIn.ExternalSearch.Providers.DuckDuckgo.Net;
using CluedIn.ExternalSearch.Providers.DuckDuckGo.Vocabularies;
using Newtonsoft.Json;
using RestSharp;
using EntityType = CluedIn.Core.Data.EntityType;
using Neo4j.Driver;
using CluedIn.ExternalSearch.Provider;
using CluedIn.Core.Data.Vocabularies.Models;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo
{
    /// <summary>A duck go external search provider.</summary>
    /// <seealso cref="T:CluedIn.ExternalSearch.ExternalSearchProviderBase"/>
    public class DuckDuckGoExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata, IConfigurableExternalSearchProvider
    {
        /**********************************************************************************************************
         * FIELDS
         **********************************************************************************************************/

        private static readonly EntityType[] DefaultAcceptedEntityTypes = { EntityType.Organization };

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        public DuckDuckGoExternalSearchProvider()
            : base(DuckDuckGoConstants.ProviderId, DefaultAcceptedEntityTypes)
        {
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        public IEnumerable<EntityType> Accepts(IDictionary<string, object> config, IProvider provider) => this.Accepts(config);

        private IEnumerable<EntityType> Accepts(IDictionary<string, object> config)
            => Accepts(new DuckDuckGoExternalSearchJobData(config));

        private IEnumerable<EntityType> Accepts(DuckDuckGoExternalSearchJobData config)
        {
            if (!string.IsNullOrWhiteSpace(config.AcceptedEntityType))
            {
                // If configured, only accept the configured entity types
                return new EntityType[] { config.AcceptedEntityType };
            }

            // Fallback to default accepted entity types
            return DefaultAcceptedEntityTypes;
        }

        private bool Accepts(DuckDuckGoExternalSearchJobData config, EntityType entityTypeToEvaluate)
        {
            var configurableAcceptedEntityTypes = this.Accepts(config).ToArray();

            return configurableAcceptedEntityTypes.Any(entityTypeToEvaluate.Is);
        }

        public IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
            => InternalBuildQueries(context, request, new DuckDuckGoExternalSearchJobData(config));

        private IEnumerable<IExternalSearchQuery> InternalBuildQueries(ExecutionContext context, IExternalSearchRequest request, DuckDuckGoExternalSearchJobData config = null)
        {
            if (!this.Accepts(config, request.EntityMetaData.EntityType))
                yield break;

            var existingResults = request.GetQueryResults<SearchResult>(this).Where(r => r.Data.Infobox != null).ToList();

            Func<string, bool> existingResultsFilter    = value => existingResults.Any(r => string.Equals(r.Data.Infobox.Meta.First().Value, value, StringComparison.InvariantCultureIgnoreCase));
            Func<string, bool> existingResultsFilter2   = value => existingResults.Any(r => r.Data.Results.Any(v => v.FirstURL.Contains(value)));
            Func<string, bool> nameFilter               = value => OrganizationFilters.NameFilter(context, value);

            // Query Input
            var entityType     = request.EntityMetaData.EntityType;

            var companyName    = new HashSet<string>();
            var companyWebsite = new HashSet<string>();

            if (!string.IsNullOrWhiteSpace(config.OrgNameKey))
            {
                companyName = request.QueryParameters.GetValue<string, HashSet<string>>(config.OrgNameKey, new HashSet<string>());
            }
            else
            {
                companyName = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName, new HashSet<string>()).ToHashSet();
            }
            if (!string.IsNullOrWhiteSpace(config.WebsiteKey))
            {
                companyWebsite = request.QueryParameters.GetValue<string, HashSet<string>>(config.WebsiteKey, new HashSet<string>());
            }
            else
            {
                companyWebsite = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, new HashSet<string>()).ToHashSet();
            }


            if (companyName != null)
            {
                var values = companyName.Select(NameNormalization.Normalize).ToHashSet();

                foreach (var value in values.Where(v => !nameFilter(v) && !existingResultsFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Name, value);
            }

            if (companyWebsite != null)
            {
                var uriHosts = companyWebsite.Where(UriUtility.IsValid)
                                             .Select(u => new Uri(u).Host.ToLowerInvariant())
                                             .Distinct();

                var domainHosts = companyWebsite.Where(v =>
                    {
                        if (UriUtility.IsValid(v))
                            return false;

                        if (!DomainName.TryParse(v, out var domain))
                            return false;

                        return true;
                    }).Distinct();

                var hosts = uriHosts.Union(domainHosts).Where(v => !existingResultsFilter2(v));

                foreach (var value in hosts)
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Name, value);
            }
        }

        public IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query, IDictionary<string, object> config, IProvider provider)
        {
            var id = query.QueryParameters[ExternalSearchQueryParameter.Name].FirstOrDefault();

            if (string.IsNullOrEmpty(id))
                yield break;

            var client = new RestClient("https://api.duckduckgo.com");

            var responseData = JsonRequestWrapper<SearchResult>(client, string.Format("?q={0}&format=json", id), Method.GET);

            if (responseData?.Infobox != null)
                yield return new ExternalSearchQueryResult<SearchResult>(query, responseData);
            else
                yield break;
        }

        public IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            var resultItem = result.As<SearchResult>();

            if (resultItem.Data.Entity != "company")
                yield break;

            if (string.IsNullOrEmpty(resultItem.Data.Heading))
                yield break;

            var code = new EntityCode(Core.Data.EntityType.Organization, CodeOrigin.CluedIn.CreateSpecific("duckDuckGo"), request.EntityMetaData.OriginEntityCode.Value);

            var clue = new Clue(code, context.Organization);

            clue.Data.OriginProviderDefinitionId = this.Id;

            this.PopulateMetadata(clue.Data.EntityData, resultItem, request, context);

            if (resultItem.Data.ImageIsLogo == 1 && resultItem.Data.Image != null)
                this.DownloadPreviewImage(context, resultItem.Data.Image, clue);

            yield return clue;
        }


        public IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            var resultItem = result.As<SearchResult>();

            if (resultItem.Data.Entity != "company")
                return null;

            return this.CreateMetadata(resultItem, request, context);
        }

        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            // Note: This needs to be cleaned up, but since config and provider is not used in GetPrimaryEntityPreviewImage this is fine.
            var dummyConfig   = new Dictionary<string, object>();
            var dummyProvider = new DefaultExternalSearchProviderProvider(context.ApplicationContext, this);

            return GetPrimaryEntityPreviewImage(context, result, request, dummyConfig, dummyProvider);
        }

        public IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            var resultItem = result.As<SearchResult>();

            if (resultItem.Data.Entity != "company")
                return null;

            if (resultItem.Data.ImageIsLogo == 1 && resultItem.Data.Image != null)
                base.DownloadPreviewImageBlob<SearchResult>(context, result, r => r.Data.Image);

            return null;
        }

        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<SearchResult> resultItem, IExternalSearchRequest request, ExecutionContext context)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem, request, context);

            return metadata;
        }

        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<SearchResult> resultItem, IExternalSearchRequest request, ExecutionContext context)
        {
            var code = new EntityCode(request.EntityMetaData.EntityType, CodeOrigin.CluedIn.CreateSpecific("duckDuckGo"), request.EntityMetaData.OriginEntityCode.Value);

            metadata.EntityType       = request.EntityMetaData.EntityType;
            metadata.Name             = request.EntityMetaData.Name;
            metadata.Description      = resultItem.Data.Abstract;
            metadata.OriginEntityCode = code;
            metadata.Codes.Add(request.EntityMetaData.OriginEntityCode);

            var uri = resultItem.Data.Results.FirstOrDefault()?.FirstURL;
            if (uri != null && UriUtility.IsValid(uri))
                metadata.Uri = new Uri(uri);

            metadata.Properties[DuckDuckGoVocabulary.Organization.Abstract]         = resultItem.Data.Abstract.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.AbstractSource]   = resultItem.Data.AbstractSource.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.AbstractText]     = resultItem.Data.AbstractText.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.AbstractURL]      = resultItem.Data.AbstractURL.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.Answer]           = resultItem.Data.Answer.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.AnswerType]       = resultItem.Data.AnswerType.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.Definition]       = resultItem.Data.Definition.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.DefinitionSource] = resultItem.Data.DefinitionSource.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.DefinitionURL]    = resultItem.Data.DefinitionURL.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.Entity]           = resultItem.Data.Entity.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.Heading]          = resultItem.Data.Heading.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.ImageHeight]      = resultItem.Data.ImageHeight.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.Image]            = resultItem.Data.Image.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.ImageIsLogo]      = resultItem.Data.ImageIsLogo.PrintIfAvailable();

            metadata.Properties[DuckDuckGoVocabulary.Organization.ImageWidth]       = resultItem.Data.ImageWidth.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.Redirect]         = resultItem.Data.Redirect.PrintIfAvailable();
            metadata.Properties[DuckDuckGoVocabulary.Organization.Type]             = resultItem.Data.Type.PrintIfAvailable();

            // Results
            metadata.Properties[DuckDuckGoVocabulary.Organization.Websites]         = JoinValues(resultItem.Data.Results, x => x?.FirstURL);

            // Related Topics
            var relatedTopics = resultItem.Data.RelatedTopics;
            for (int i = 0; i < relatedTopics.Count(); i++)
            {
                metadata.Properties[DuckDuckGoVocabulary.RelatedTopics.KeyPrefix + DuckDuckGoVocabulary.RelatedTopics.KeySeparator + $"{i}.text"]     = relatedTopics[i].Text.PrintIfAvailable();
                metadata.Properties[DuckDuckGoVocabulary.RelatedTopics.KeyPrefix + DuckDuckGoVocabulary.RelatedTopics.KeySeparator + $"{i}.firstUrl"] = relatedTopics[i].FirstURL.PrintIfAvailable();
                metadata.Properties[DuckDuckGoVocabulary.RelatedTopics.KeyPrefix + DuckDuckGoVocabulary.RelatedTopics.KeySeparator + $"{i}.icon"]     = relatedTopics[i].Icon.URL.PrintIfAvailable();
            }

            // Infobox
            var vocabularyRepositoryType = typeof(CluedIn.Integration.PrivateServices.PrivateServicesComponent).Assembly.GetType("CluedIn.Integration.PrivateServices.Vocabularies.IPrivateVocabularyRepository");

            var vocabRepository = context.ApplicationContext.Container.Resolve(vocabularyRepositoryType);

            var getVocabMethodInfo = vocabRepository.GetType().GetMethod("GetVocabularyByKeyPrefix");
            var getVocabKeyMethodInfo = vocabRepository.GetType().GetMethod("GetVocabularyKeyByFullName");
            var addVocabMethodInfo = vocabRepository.GetType().GetMethod("AddVocabulary");
            var addVocabKeyMethodInfo = vocabRepository.GetType().GetMethod("AddVocabularyKey");

            Guid vocabId = Guid.Empty;

            IVocabulary existingVocab = (IVocabulary)getVocabMethodInfo.Invoke(vocabRepository, new object[] { "duckDuckGo.organization", false });

            if (existingVocab == null || existingVocab?.KeyPrefix != "duckDuckGo.organization") {
                var newVocab = new AddVocabularyModel { VocabularyName = "DuckDuckGo Organization", KeyPrefix = "duckDuckGo.organization", Grouping = EntityType.Organization };
                vocabId = (Guid)addVocabMethodInfo.Invoke(vocabRepository, new object[] { newVocab, Guid.Empty.ToString(), context.Organization.Id });
            } else
            {
                vocabId = existingVocab.VocabularyId;
            }

            foreach (var content in resultItem.Data.Infobox.Content)
            {
                string label = FormatLabelToProperty(content.Label);
                VocabularyKey existingVocabKey = (VocabularyKey)getVocabKeyMethodInfo.Invoke(vocabRepository, new object[] { label });
                if (label != null && existingVocabKey == null)
                {
                    var newVocabKey = new AddVocabularyKeyModel { VocabularyId = vocabId, DisplayName = "infobox-" + label, GroupName = "DuckDuckGo Organization Infobox", Name = "infobox" + DuckDuckGoVocabulary.Infobox.KeySeparator + label, DataType = VocabularyKeyDataType.Text, IsVisible = true, Storage = VocabularyKeyStorage.Keyword };
                    addVocabKeyMethodInfo.Invoke(vocabRepository, new object[] { newVocabKey, context, Guid.Empty.ToString(), true });
                    metadata.Properties[DuckDuckGoVocabulary.Infobox.KeyPrefix + DuckDuckGoVocabulary.Infobox.KeySeparator + label] = content.Value.PrintIfAvailable();
                }
            }

            metadata.Codes.Add(code);
        }

        /// <summary>
        /// Wrapper around a request to ensure proper deserialization of the JSON.
        /// </summary>
        /// <typeparam name="T">The model</typeparam>
        /// <param name="client">An IRestClient for the request</param>
        /// <param name="parameter">The parameters for the request</param>
        /// <param name="callback">Pass additional information to the request (e.g. headers)</param>
        /// <returns>A deserialized object</returns>
        private static T JsonRequestWrapper<T>(IRestClient client, string parameter, Method method, Action<IRestRequest> callback = null)
        {
            var request = new RestRequest(parameter, method);

            callback?.Invoke(request);

            var response = client.ExecuteTaskAsync(request).Result;

            T responseData;
            if (response.StatusCode == HttpStatusCode.OK)
                responseData = JsonConvert.DeserializeObject<T>(response.Content);
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                responseData = default(T);
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; Content: " + response.Content);

            return responseData;
        }

        /// <summary>
        /// Formats the label so it fits the style of the properties (e.g. "Company type" -> "companyType")
        /// </summary>
        /// <param name="label">The label to format</param>
        /// <returns>The formatted label</returns>
        private static string FormatLabelToProperty(string label)
        {
            return String.Join("", label.Split(' ').Select((x, i) => i == 0 ? x.ToLower() : FirstCharacterToUpper(x)));
        }

        /// <summary>
        /// Capitalizes the first character in the string
        /// </summary>
        /// <param name="text">The text that should be capitalized</param>
        /// <returns>The string with the first character capitalized</returns>
        private static string FirstCharacterToUpper(string text)
        {
            return $"{char.ToUpper(text[0])}{text.Substring(1)}";
        }

        /// <summary>
        /// Joins the properties of a list into a string
        /// </summary>
        /// <typeparam name="T">The object</typeparam>
        /// <param name="items">The list of objects to be joined</param>
        /// <param name="property">The property that should be joined</param>
        /// <returns>A comma separated string containing the properties</returns>
        private static string JoinValues<T>(List<T> items, Func<T, string> property, string separator = ";")
        {
            if (items != null && items.Any())
            {
                return String.Join(separator, items.Where(x => !String.IsNullOrEmpty(property(x))).ToList().ConvertAll(x => property(x)));
            }

            return null;
        }

        // Since this is a configurable external search provider, theses methods should never be called
        public override bool Accepts(EntityType entityType) => throw new NotSupportedException();
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request) => throw new NotSupportedException();
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query) => throw new NotSupportedException();
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request) => throw new NotSupportedException();
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request) => throw new NotSupportedException();

        /**********************************************************************************************************
         * PROPERTIES
         **********************************************************************************************************/

        public string Icon { get; } = DuckDuckGoConstants.Icon;
        public string Domain { get; } = DuckDuckGoConstants.Domain;
        public string About { get; } = DuckDuckGoConstants.About;
        public AuthMethods AuthMethods { get; } = null;
        public IEnumerable<Control> Properties { get; } = null;
        public Guide Guide { get; } = null;
        public IntegrationType Type { get; } = IntegrationType.Cloud;
    }
}
