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
using CluedIn.Core.Connectors;
using CluedIn.Crawling.Helpers;
using CluedIn.Core.Data.Vocabularies.Models;
using CluedIn.Core.Data.Vocabularies;
using CluedIn.ExternalSearch.Provider;
using CluedIn.ExternalSearch.Filters;
using CluedIn.ExternalSearch.Providers.DuckDuckgo;
using CluedIn.ExternalSearch.Providers.DuckDuckGo.Model;
using CluedIn.ExternalSearch.Providers.DuckDuckgo.Net;
using CluedIn.ExternalSearch.Providers.DuckDuckGo.Vocabularies;
using CluedIn.ExternalSearch.Providers.DuckDuckgo.Helper;
using CluedIn.ExternalSearch.Providers.DuckDuckgo.Services;
using EntityType = CluedIn.Core.Data.EntityType;
using Newtonsoft.Json;
using RestSharp;
using System.Web;
using System.Text.RegularExpressions;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo
{
    /// <summary>A duck go external search provider.</summary>
    /// <seealso cref="T:CluedIn.ExternalSearch.ExternalSearchProviderBase"/>
    public class DuckDuckGoExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata, IConfigurableExternalSearchProvider, IExternalSearchProviderWithVerifyConnection
    {
        /**********************************************************************************************************
         * FIELDS
         **********************************************************************************************************/

        private static readonly EntityType[] DefaultAcceptedEntityTypes = { EntityType.Organization };
        private const int ExportEntitiesLockInMilliseconds = 100;
        private const string StreamIdKey = "StreamId";
        private const string DataTimeKey = "DataTime";

        public struct ResultType
        {
            public const string RelatedTopics = "RelatedTopics";
            public const string Infobox = "Infobox";
        }

        public struct RelatedTopicsType
        {
            public const string FirstUrl = "firstUrl";
            public const string Text = "text";
            public const string Icon = "icon";
        }
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

            return configurableAcceptedEntityTypes.SafeEnumerate().Any(entityTypeToEvaluate.Is);
        }

        public IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
            => InternalBuildQueries(context, request, new DuckDuckGoExternalSearchJobData(config));

        private IEnumerable<IExternalSearchQuery> InternalBuildQueries(ExecutionContext context, IExternalSearchRequest request, DuckDuckGoExternalSearchJobData config = null)
        {
            if (!this.Accepts(config, request.EntityMetaData.EntityType))
                yield break;

            var existingResults = request.GetQueryResults<SearchResult>(this).Where(r => r.Data.Infobox != null).ToList();

            Func<string, bool> existingResultsFilter    = value => existingResults.SafeEnumerate().Any(r => string.Equals(r.Data.Infobox.Meta.First().Value, value, StringComparison.InvariantCultureIgnoreCase));
            Func<string, bool> existingResultsFilter2   = value => existingResults.SafeEnumerate().Any(r => r.Data.Results.SafeEnumerate().Any(v => v.FirstURL.Contains(value)));
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
            var name = query.QueryParameters[ExternalSearchQueryParameter.Name].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(name))
                yield break;

            var client = new RestClient("https://api.duckduckgo.com")
            {
                // TODO rotating the useragent can help with throttling
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36 Edg/130.0.0.0"
            };

            foreach (var searchName in GetSearchVariants(name.Trim()))
            {
                var responseData = JsonRequestWrapper(client, searchName, Method.GET);

                if (responseData?.Infobox == null) continue;

                yield return new ExternalSearchQueryResult<SearchResult>(query, responseData);
                break;
            }
        }

        private IEnumerable<string> GetSearchVariants(string name)
        {
            yield return name;
            yield return $"{name} company";
            yield return $"{name} corporation";
        }

        public IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            var resultItem = result.As<SearchResult>();

            if (resultItem.Data.Entity != "company")
                yield break;

            if (string.IsNullOrEmpty(resultItem.Data.Heading))
                yield break;

            var clue = new Clue(request.EntityMetaData.OriginEntityCode, context.Organization);

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

        public ConnectionVerificationResult VerifyConnection(ExecutionContext context, IReadOnlyDictionary<string, object> config)
        {
            var client = new RestClient("https://api.duckduckgo.com");
            var queryParameters = HttpUtility.ParseQueryString("");
            queryParameters.Add("format", "json");
            queryParameters.Add("timestamp", DateTime.Now.Ticks.ToString());    // potentially helps with throttling

            var request = new RestRequest($"?{queryParameters}", Method.GET);
            var response = client.Execute(request);

            return ConstructVerifyConnectionResponse(response);
        }

        private ConnectionVerificationResult ConstructVerifyConnectionResponse(IRestResponse response)
        {
            var errorMessageBase = $"{DuckDuckGoConstants.ProviderName} returned \"{(int)response.StatusCode} {response.StatusDescription}\".";
            if (response.ErrorException != null)
                return new ConnectionVerificationResult(false, $"{errorMessageBase} {(!string.IsNullOrWhiteSpace(response.ErrorException.Message) ? response.ErrorException.Message : "This could be due to breaking changes in the external system")}.");

            if (response.StatusCode is HttpStatusCode.Unauthorized)
                return new ConnectionVerificationResult(false, $"{errorMessageBase} This could be due to invalid API key.");

            var regex = new Regex(@"\<(html|head|body|div|span|img|p\>|a href)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            var isHtml = regex.IsMatch(response.Content);

            string errorMessage = response.IsSuccessful ? string.Empty
                : string.IsNullOrWhiteSpace(response.Content) || isHtml
                    ? $"{errorMessageBase} This could be due to breaking changes in the external system."
                    : $"{errorMessageBase} {response.Content}.";

            return new ConnectionVerificationResult(response.IsSuccessful, errorMessage);
        }

        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<SearchResult> resultItem, IExternalSearchRequest request, ExecutionContext context)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem, request, context);

            return metadata;
        }

        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<SearchResult> resultItem, IExternalSearchRequest request, ExecutionContext context)
        {
            metadata.EntityType       = request.EntityMetaData.EntityType;
            metadata.Name             = request.EntityMetaData.Name;
            metadata.Description      = resultItem.Data.Abstract;
            metadata.OriginEntityCode = request.EntityMetaData.OriginEntityCode;

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


            var vocabId = GetOrCreateDuckDuckGoVocabularyId(context);

            // Related Topics
            ProcessRelatedTopicsVocabulary(metadata, resultItem, context, vocabId);

            // Infobox
            ProcessInfoboxVocabulary(metadata, resultItem, context, vocabId);
        }

        private void ProcessRelatedTopicsVocabulary(IEntityMetadata metadata, IExternalSearchQueryResult<SearchResult> resultItem, ExecutionContext context, Guid vocabId)
        {
            var relatedTopics = resultItem.Data.RelatedTopics;
            for (int i = 0; i < relatedTopics.Count; i++)
            {
                if (!string.IsNullOrEmpty(relatedTopics[i].FirstURL))
                {
                    CreateVocabularyKeyIfNecessary(context, vocabId, ResultType.RelatedTopics, i, null , RelatedTopicsType.FirstUrl);
                    metadata.Properties[DuckDuckGoVocabulary.RelatedTopics.KeyPrefix + DuckDuckGoVocabulary.RelatedTopics.KeySeparator + $"{i}.firstUrl"] = relatedTopics[i].FirstURL.PrintIfAvailable();
                }

                if (!string.IsNullOrEmpty(relatedTopics[i].Text))
                {
                    CreateVocabularyKeyIfNecessary(context, vocabId, ResultType.RelatedTopics, i, null, RelatedTopicsType.Text);
                    metadata.Properties[DuckDuckGoVocabulary.RelatedTopics.KeyPrefix + DuckDuckGoVocabulary.RelatedTopics.KeySeparator + $"{i}.text"] = relatedTopics[i].Text.PrintIfAvailable();
                }

                if (relatedTopics[i].Icon != null)
                {
                    CreateVocabularyKeyIfNecessary(context, vocabId, ResultType.RelatedTopics, i, null, RelatedTopicsType.Icon);
                    metadata.Properties[DuckDuckGoVocabulary.RelatedTopics.KeyPrefix + DuckDuckGoVocabulary.RelatedTopics.KeySeparator + $"{i}.icon"] = relatedTopics[i].Icon.URL.PrintIfAvailable();
                }
            }

        }
        private void ProcessInfoboxVocabulary(IEntityMetadata metadata, IExternalSearchQueryResult<SearchResult> resultItem, ExecutionContext context, Guid vocabId)
        {
            foreach (var content in resultItem.Data.Infobox.Content)
            {
                var label = FormatLabelToProperty(content.Label);

                if (label == null) continue;

                CreateVocabularyKeyIfNecessary(context, vocabId, ResultType.Infobox, null, label, null);

                metadata.Properties[DuckDuckGoVocabulary.Infobox.KeyPrefix + DuckDuckGoVocabulary.Infobox.KeySeparator + label] = content.Value.PrintIfAvailable();
            }
        }

        private static void CreateVocabularyKeyIfNecessary(ExecutionContext context, Guid vocabId, string keyType, int? count = null, string label = null, string relatedTopicsType = null)
        {
            string cacheKey;
            if (keyType == ResultType.RelatedTopics)
            {
                cacheKey = $"DuckDuckGo_CreateVocabularyKeyIfNecessary_RelatedTopics_{count}_{relatedTopicsType}";
            }
            else if (keyType == ResultType.Infobox)
            {
                cacheKey = $"DuckDuckGo_CreateVocabularyKeyIfNecessary_Infobox_{label}";
            }
            else
            {
                return;
            }

            object cached = context.ApplicationContext.System.Cache.GetItem<object>(cacheKey);
            if (cached != null) return;

            using (LockHelper.GetDistributedLockAsync(context.ApplicationContext, "DuckDuckGo_CreateVocab_Lock", TimeSpan.FromMinutes(1)).GetAwaiter().GetResult())
            {
                var vocabRepository = context.ApplicationContext.Container.Resolve<IVocabularyRepository>();

                VocabularyKey existingVocabKey;
                if (keyType == ResultType.RelatedTopics)
                {
                    existingVocabKey = vocabRepository.GetVocabularyKeyByFullName(context, DuckDuckGoVocabulary.RelatedTopics.KeyPrefix + DuckDuckGoVocabulary.RelatedTopics.KeySeparator + count + DuckDuckGoVocabulary.RelatedTopics.KeySeparator + relatedTopicsType);
                    if (existingVocabKey == null)
                    {
                        string displayNamePostFix = relatedTopicsType switch
                        {
                            RelatedTopicsType.FirstUrl => "Url",
                            RelatedTopicsType.Text => "Text",
                            RelatedTopicsType.Icon => "Icon",
                            _ => ""
                        };

                        var newVocabKey = new AddVocabularyKeyModel
                        {
                            VocabularyId = vocabId,
                            DisplayName = $"Related Topics {count} {displayNamePostFix}",
                            GroupName = "DuckDuckGo Organization Related Topics",
                            Name = $"relatedTopics.{count}" + DuckDuckGoVocabulary.RelatedTopics.KeySeparator + relatedTopicsType,
                            DataType = VocabularyKeyDataType.Text,
                            IsVisible = true,
                            Storage = VocabularyKeyStorage.Keyword
                        };
                        var vocabKeyId = vocabRepository.AddVocabularyKey(newVocabKey, context, Guid.Empty.ToString(), true).GetAwaiter().GetResult();
                        vocabRepository.ActivateVocabularyKey(context, vocabKeyId).GetAwaiter().GetResult();

                    }
                }
                else if (keyType == ResultType.Infobox)
                {
                    existingVocabKey = vocabRepository.GetVocabularyKeyByFullName(context, DuckDuckGoVocabulary.Infobox.KeyPrefix + DuckDuckGoVocabulary.Infobox.KeySeparator + label);
                    if (existingVocabKey == null)
                    {
                        var newVocabKey = new AddVocabularyKeyModel
                        {
                            VocabularyId = vocabId,
                            DisplayName = "Infobox-" + label,
                            GroupName = "DuckDuckGo Organization Infobox",
                            Name = "infobox" + DuckDuckGoVocabulary.Infobox.KeySeparator + label,
                            DataType = VocabularyKeyDataType.Text,
                            IsVisible = true,
                            Storage = VocabularyKeyStorage.Keyword
                        };
                        var vocabKeyId = vocabRepository.AddVocabularyKey(newVocabKey, context, Guid.Empty.ToString(), true).GetAwaiter().GetResult();
                        vocabRepository.ActivateVocabularyKey(context, vocabKeyId).GetAwaiter().GetResult();
                    }
                }
                context.ApplicationContext.System.Cache.SetItem(cacheKey, new object(), DateTimeOffset.Now.AddMinutes(1));
            }
        }

        private static Guid GetOrCreateDuckDuckGoVocabularyId(ExecutionContext context)
        {
            var cacheKey = "DuckDuckGo-GetExistingVocabulary";
            var cached = context.ApplicationContext.System.Cache.GetItem<object>(cacheKey);

            if (cached != null) return (Guid)cached;

            using (LockHelper.GetDistributedLockAsync(context.ApplicationContext, "DuckDuckGo_CreateVocab_Lock", TimeSpan.FromMinutes(1)).GetAwaiter().GetResult())
            {
                var vocabularyRepository = context.ApplicationContext.Container.Resolve<IVocabularyRepository>();

                var vocab = vocabularyRepository.GetVocabularyByKeyPrefix(context, "duckDuckGo.organization", false );

                Guid vocabId;
                if (vocab == null)
                {
                    var newVocab = new AddVocabularyModel { VocabularyName = "DuckDuckGo Organization", KeyPrefix = "duckDuckGo.organization", Grouping = EntityType.Organization };
                    vocabId = vocabularyRepository.AddVocabulary(context, newVocab, Guid.Empty.ToString(), context.Organization.Id).GetAwaiter().GetResult();
                    vocabularyRepository.ActivateVocabulary(context, vocabId).GetAwaiter().GetResult();
                }
                else
                {
                    vocabId = vocab.VocabularyId;
                }

                context.ApplicationContext.System.Cache.SetItem(cacheKey, (object)vocabId, DateTimeOffset.Now.AddMinutes(1));

                return vocabId;
            }
        }

        /// <summary>
        /// Wrapper around a request to ensure proper deserialization of the JSON.
        /// </summary>
        private static SearchResult JsonRequestWrapper(IRestClient client, string name, Method method)
        {
            var queryParameters = HttpUtility.ParseQueryString("");
            queryParameters.Add("q", name);
            queryParameters.Add("format", "json");
            queryParameters.Add("timestamp", DateTime.Now.Ticks.ToString());    // potentially helps with throttling

            var request = new RestRequest($"?{queryParameters}", method);
            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                System.Threading.Thread.Sleep(1000);   // sleeping as if there is an error with the remote server we don't want to burn through the queue
                throw new ApplicationException($"Could not execute external search query - {response.ErrorException}");
            }

            if (response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound)
                return null;

            var content = response.Content;

            if (response.IsSuccessful)
            {
                var responseData = JsonConvert.DeserializeObject<SearchResult>(content);

                if (responseData.Infobox != null) return responseData;

                if (response.StatusCode != HttpStatusCode.Accepted) return responseData;

                System.Threading.Thread.Sleep(30000);   // sleep as we are throttled
            }

            throw new ApplicationException($"Could not execute external search query - StatusCode:{response.StatusCode}; Content: {content}");
        }

        /// <summary>
        /// Formats the label so it fits the style of the properties (e.g. "Company type" -> "companyType")
        /// </summary>
        /// <param name="label">The label to format</param>
        /// <returns>The formatted label</returns>
        private static string FormatLabelToProperty(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return null;

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
            if (items != null && items.SafeEnumerate().Any())
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
