// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoVocabulary.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the duck go vocabulary class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo.Vocabularies
{
    /// <summary>A duck go vocabulary.</summary>
    public static class DuckDuckGoVocabulary
    {
        public static DuckDuckGoOrganizationVocabulary  Organization    { get; } = new DuckDuckGoOrganizationVocabulary();
        public static DuckDuckGoInfoboxVocabulary       Infobox         { get; } = new DuckDuckGoInfoboxVocabulary();
        public static DuckDuckGoRelatedTopicsVocabulary RelatedTopics   { get; } = new DuckDuckGoRelatedTopicsVocabulary();
    }
}
