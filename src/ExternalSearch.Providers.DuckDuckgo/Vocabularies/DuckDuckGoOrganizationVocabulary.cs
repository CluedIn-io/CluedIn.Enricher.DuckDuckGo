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
    }
}
