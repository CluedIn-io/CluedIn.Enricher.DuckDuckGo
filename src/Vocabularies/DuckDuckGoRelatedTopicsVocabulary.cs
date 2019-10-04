// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoRelatedTopicsVocabulary.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the DuckDuckGo Related Topics vocabulary class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo.Vocabularies
{
    public class DuckDuckGoRelatedTopicsVocabulary : DynamicVocabulary
    {
        public DuckDuckGoRelatedTopicsVocabulary()
        {
            this.VocabularyName        = "DuckDuckGo Organization Related Topics Properties";
            this.KeyPrefix             = "duckDuckGo.organization.relatedTopics";
            this.KeySeparator          = "-";
            this.Grouping              = EntityType.Unknown;
            this.ShowInApplication     = true;
            this.ShowUrisInApplication = false;
        }
    }
}