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
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Filters;
using CluedIn.ExternalSearch.Providers.DuckDuckGo.Model;
using CluedIn.ExternalSearch.Providers.DuckDuckGo.Vocabularies;
using DomainNameParser;
using Newtonsoft.Json;
using RestSharp;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo
{
    /// <summary>A duck go external search provider.</summary>
    /// <seealso cref="T:CluedIn.ExternalSearch.ExternalSearchProviderBase"/>
    public class DuckDuckGoExternalSearchProvider : ExternalSearchProviderBase
    {
        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        // TODO: Move Magic GUID to constants
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DuckDuckGoExternalSearchProvider" /> class.
        /// </summary>
        public DuckDuckGoExternalSearchProvider()
            : base(new Guid("{C7DDBEA4-D5A2-4F25-B2A0-EBFD36D2E8D6}"), EntityType.Organization)
        {
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        /// <summary>Builds the queries.</summary>
        /// <param name="context">The context.</param>
        /// <param name="request">The request.</param>
        /// <returns>The search queries.</returns>
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            if (!this.Accepts(request.EntityMetaData.EntityType))
                yield break;

            var existingResults = request.GetQueryResults<SearchResult>(this).Where(r => r.Data.Infobox != null).ToList();

            Func<string, bool> existingResultsFilter    = value => existingResults.Any(r => string.Equals(r.Data.Infobox.Meta.First().Value, value, StringComparison.InvariantCultureIgnoreCase));
            Func<string, bool> existingResultsFilter2   = value => existingResults.Any(r => r.Data.Results.Any(v => v.FirstURL.Contains(value)));
            Func<string, bool> nameFilter               = value => OrganizationFilters.NameFilter(context, value);

            // Query Input
            var entityType     = request.EntityMetaData.EntityType;
            var companyName    = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName, new HashSet<string>());
            var companyWebsite = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, new HashSet<string>());

            if (companyName != null)
            {
                var values = companyName.Select(NameNormalization.Normalize).ToHashSetEx();

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

                        DomainName domain;
                        if (!DomainName.TryParse(v, out domain))
                            return false;

                        return true;
                    }).Distinct();

                var hosts = uriHosts.Union(domainHosts).Where(v => !existingResultsFilter2(v));

                foreach (var value in hosts)
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Name, value);
            }
        }

        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
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

        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<SearchResult>();

            if (resultItem.Data.Entity != "company")
                yield break;

            if (string.IsNullOrEmpty(resultItem.Data.Heading))
                yield break;

            var code = new EntityCode(EntityType.Organization, CodeOrigin.CluedIn.CreateSpecific("duckDuckGo"), resultItem.Data.Heading);

            var clue = new Clue(code, context.Organization);

            clue.Data.OriginProviderDefinitionId = this.Id;

            this.PopulateMetadata(clue.Data.EntityData, resultItem);

            if (resultItem.Data.ImageIsLogo == 1 && resultItem.Data.Image != null)
                this.DownloadPreviewImage(context, resultItem.Data.Image, clue);

            yield return clue;
        }

        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<SearchResult>();

            if (resultItem.Data.Entity != "company")
                return null;

            return this.CreateMetadata(resultItem);
        }

        /// <summary>Gets the preview image.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The preview image.</returns>
        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<SearchResult>();

            if (resultItem.Data.Entity != "company")
                return null;

            if (resultItem.Data.ImageIsLogo == 1 && resultItem.Data.Image != null)
                base.DownloadPreviewImageBlob<SearchResult>(context, result, r => r.Data.Image);

            return null;
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<SearchResult> resultItem)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem);

            return metadata;
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<SearchResult> resultItem)
        {
            var code = new EntityCode(EntityType.Organization, CodeOrigin.CluedIn.CreateSpecific("duckDuckGo"), resultItem.Data.Heading);

            metadata.EntityType       = EntityType.Organization;
            metadata.Name             = resultItem.Data.Heading;
            metadata.Description      = resultItem.Data.Abstract;
            metadata.OriginEntityCode = code;

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
            foreach (var content in resultItem.Data.Infobox.Content)
            {
                string label = FormatLabelToProperty(content.Label);

                switch (label)
                {
                    case "industry":
                        metadata.Properties[DuckDuckGoVocabulary.Organization.Industry] = content.Value.PrintIfAvailable();
                        break;
                    case "founded":
                        metadata.Properties[DuckDuckGoVocabulary.Organization.Founded] = content.Value.PrintIfAvailable();
                        break;
                    case "revenue":
                        metadata.Properties[DuckDuckGoVocabulary.Organization.Revenue] = content.Value.PrintIfAvailable();
                        break;
                    case "employees":
                        metadata.Properties[DuckDuckGoVocabulary.Organization.Employees] = content.Value.PrintIfAvailable();
                        break;
                    case "gitHubProfile":
                        metadata.Properties[DuckDuckGoVocabulary.Organization.GitHubProfile] = content.Value.PrintIfAvailable();
                        break;
                    case "twitterProfile":
                        metadata.Properties[DuckDuckGoVocabulary.Organization.TwitterProfile] = content.Value.PrintIfAvailable();
                        break;
                    case "facebookProfile":
                        metadata.Properties[DuckDuckGoVocabulary.Organization.FacebookProfile] = content.Value.PrintIfAvailable();
                        break;
                    case "instagramProfile":
                        metadata.Properties[DuckDuckGoVocabulary.Organization.InstagramProfile] = content.Value.PrintIfAvailable();
                        break;
                    case "youtubeChannel":
                        metadata.Properties[DuckDuckGoVocabulary.Organization.YouTubeChannel] = content.Value.PrintIfAvailable();
                        break;
                    default:
                        metadata.Properties[DuckDuckGoVocabulary.Infobox.KeyPrefix + DuckDuckGoVocabulary.Infobox.KeySeparator + label] = content.Value.PrintIfAvailable();
                        break;
                }
            }

            metadata.Codes.Add(code);
        }

        /// <summary>
        /// Wrapper around a request to ensure propper deserialization of the JSON.
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
        /// Formarts the label so it fits the style of the properties (e.g. "Company type" -> "companyType")
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
    }
}
