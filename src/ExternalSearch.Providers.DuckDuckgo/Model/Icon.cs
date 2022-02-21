// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoResponse.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the duck go response class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.DuckDuckGo.Model
{
    public class Icon
    {

        [JsonProperty("Height")]
        public int? Height { get; set; }

        [JsonProperty("Width")]
        public int? Width { get; set; }

        [JsonProperty("URL")]
        public string URL { get; set; }
    }
}
