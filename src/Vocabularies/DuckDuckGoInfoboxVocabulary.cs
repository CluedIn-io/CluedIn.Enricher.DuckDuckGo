// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoInfoboxVocabulary.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the DuckDuckGo Infobox vocabulary class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo.Vocabularies
{
    public class DuckDuckGoInfoboxVocabulary : DynamicVocabulary
    {
        public DuckDuckGoInfoboxVocabulary()
        {
            this.VocabularyName        = "DuckDuckGo Organization Infobox Properties";
            this.KeyPrefix             = "duckDuckGo.organization.infobox";
            this.KeySeparator          = "-";
            this.Grouping              = EntityType.Organization;
            this.ShowInApplication     = true;
            this.ShowUrisInApplication = false;
        }
    }
}