// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoOrganizationVocabulary.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the duck go organization vocabulary class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;
using CluedIn.Core.Data.Vocabularies.CluedIn;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo.Vocabularies
{
    public class DuckDuckGoOrganizationVocabulary : SimpleVocabulary
    {
        public DuckDuckGoOrganizationVocabulary()
        {
            this.VocabularyName = "DuckDuckGo Organization";
            this.KeyPrefix      = "duckDuckGo.organization";
            this.KeySeparator   = ".";
            this.Grouping       = EntityType.Organization;

            this.Score            = this.Add(new VocabularyKey("score"));
            this.Abstract         = this.Add(new VocabularyKey("abstract"));
            this.AbstractSource   = this.Add(new VocabularyKey("abstractSource"));
            this.AbstractText     = this.Add(new VocabularyKey("abstractText"));
            this.ImageHeight      = this.Add(new VocabularyKey("imageHeight"));
            this.AbstractURL      = this.Add(new VocabularyKey("abstractURL"));
            this.ImageWidth       = this.Add(new VocabularyKey("imageWidth"));
            this.Heading          = this.Add(new VocabularyKey("heading"));
            this.DefinitionSource = this.Add(new VocabularyKey("definitionSource"));
            this.Definition       = this.Add(new VocabularyKey("definition"));
            this.Answer           = this.Add(new VocabularyKey("answer"));
            this.AnswerType       = this.Add(new VocabularyKey("answerType"));
            this.Redirect         = this.Add(new VocabularyKey("redirect"));
            this.Type             = this.Add(new VocabularyKey("type"));
            this.DefinitionURL    = this.Add(new VocabularyKey("definitionURL"));
            this.Entity           = this.Add(new VocabularyKey("entity"));
            this.Image            = this.Add(new VocabularyKey("image"));
            this.ImageIsLogo      = this.Add(new VocabularyKey("imageIsLogo"));

            // Results
            this.Websites         = this.Add(new VocabularyKey("websites"));

            // Infobox
            this.Industry            = this.Add(new VocabularyKey("industry"));
            this.Founded             = this.Add(new VocabularyKey("founded"));
            this.Revenue             = this.Add(new VocabularyKey("revenue"));
            this.Employees           = this.Add(new VocabularyKey("employees"));
            this.GitHubProfile       = this.Add(new VocabularyKey("gitHubProfile"));
            this.TwitterProfile      = this.Add(new VocabularyKey("twitterProfile"));
            this.FacebookProfile     = this.Add(new VocabularyKey("facebookProfile"));
            this.InstagramProfile    = this.Add(new VocabularyKey("instagramProfile"));
            this.YouTubeChannel      = this.Add(new VocabularyKey("youtubeChannel"));
            this.AreaServed          = this.Add(new VocabularyKey("areaServed"));
            this.FormerlyCalled      = this.Add(new VocabularyKey("formerlyCalled"));
            this.Founders            = this.Add(new VocabularyKey("founders"));
            this.ImdbID              = this.Add(new VocabularyKey("imdbId"));
            this.InstanceOf          = this.Add(new VocabularyKey("instanceOf"));
            this.KeyPeople           = this.Add(new VocabularyKey("keyPeople"));
            this.Parent              = this.Add(new VocabularyKey("parent"));
            this.Products            = this.Add(new VocabularyKey("products"));
            this.Subsidiaries        = this.Add(new VocabularyKey("subsidiaries"));
            this.TradedAs            = this.Add(new VocabularyKey("tradedAs"));
            this.InfoboxType         = this.Add(new VocabularyKey("type2"));
            this.WikidataAliases     = this.Add(new VocabularyKey("wikidataAliases"));
            this.WikidataDescription = this.Add(new VocabularyKey("wikidataDescription"));
            this.WikidataId          = this.Add(new VocabularyKey("wikidataId"));
            this.WikidataLabel       = this.Add(new VocabularyKey("wikidataLabel"));

            // Related Topics
            for(int i = 0; i <= 50; i++)
            {
                this.Add(new DuckDuckGoRelatedTopicsVocabulary().AsCompositeKey($"relatedTopics-{i}.firstUrl", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Add(new DuckDuckGoRelatedTopicsVocabulary().AsCompositeKey($"relatedTopics-{i}.text", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Add(new DuckDuckGoRelatedTopicsVocabulary().AsCompositeKey($"relatedTopics-{i}.icon", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
            }
        }

        public VocabularyKey Score { get; set; }
        public VocabularyKey Abstract { get; internal set; }
        public VocabularyKey AbstractSource { get; internal set; }
        public VocabularyKey AbstractText { get; internal set; }
        public VocabularyKey ImageHeight { get; internal set; }
        public VocabularyKey AbstractURL { get; internal set; }
        public VocabularyKey ImageWidth { get; internal set; }
        public VocabularyKey Heading { get; internal set; }
        public VocabularyKey DefinitionSource { get; internal set; }
        public VocabularyKey Definition { get; internal set; }
        public VocabularyKey Answer { get; internal set; }
        public VocabularyKey AnswerType { get; internal set; }
        public VocabularyKey Redirect { get; internal set; }
        public VocabularyKey Type { get; internal set; }
        public VocabularyKey DefinitionURL { get; internal set; }
        public VocabularyKey Entity { get; internal set; }
        public VocabularyKey Image { get; internal set; }
        public VocabularyKey ImageIsLogo { get; internal set; }

        // Results
        public VocabularyKey Websites { get; internal set; }

        // Infobox
        public VocabularyKey Industry { get; internal set; }
        public VocabularyKey Founded { get; internal set; }
        public VocabularyKey Revenue { get; internal set; }
        public VocabularyKey Employees { get; internal set; }
        public VocabularyKey GitHubProfile { get; internal set; }
        public VocabularyKey TwitterProfile { get; internal set; }
        public VocabularyKey FacebookProfile { get; internal set; }
        public VocabularyKey InstagramProfile { get; internal set; }
        public VocabularyKey YouTubeChannel { get; internal set; }
        public VocabularyKey AreaServed { get; internal set; }
        public VocabularyKey FormerlyCalled { get; internal set; }
        public VocabularyKey Founders { get; internal set; }
        public VocabularyKey ImdbID { get; internal set; }
        public VocabularyKey InstanceOf { get; internal set; }
        public VocabularyKey KeyPeople { get; internal set; }
        public VocabularyKey Parent { get; internal set; }
        public VocabularyKey Products { get; internal set; }
        public VocabularyKey Subsidiaries { get; internal set; }
        public VocabularyKey TradedAs { get; internal set; }
        public VocabularyKey InfoboxType { get; internal set; }
        public VocabularyKey WikidataAliases { get; internal set; }
        public VocabularyKey WikidataDescription { get; internal set; }
        public VocabularyKey WikidataId { get; internal set; }
        public VocabularyKey WikidataLabel { get; internal set; }
    }
}
