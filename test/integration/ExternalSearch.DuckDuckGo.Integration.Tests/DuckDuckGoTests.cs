// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoTests.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the duck go tests class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Messages.Processing;
using CluedIn.ExternalSearch.Providers.DuckDuckGo;
using CluedIn.Testing.Base.ExternalSearch;
using Moq;
using Xunit;

namespace ExternalSearch.DuckDuckGo.Integration.Tests
{
    public class DuckDuckGoTests : BaseExternalSearchTest<DuckDuckGoExternalSearchProvider>
    {
        [Theory(Skip = "Failed Mock exception. GitHub Issue 829 - ref https://github.com/CluedIn-io/CluedIn/issues/829")]
        [InlineData("Sitecore", "https://www.sitecore.com")]
        [InlineData("Microsoft", "https://www.microsoft.com")]
        [InlineData("Nordea", null)]
        [InlineData(null, "nordea.dk")]
        [InlineData(null, "nordea.com")]
        public void TestClueProduction([CanBeNull] string name, [CanBeNull] string website)
        {
            var properties = new EntityMetadataPart();

            if (name != null)
                properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName, name);

            if (website != null)
                properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, website);

            IEntityMetadata entityMetadata = new EntityMetadataPart() {
                Name = name,
                EntityType = EntityType.Organization,
                Properties = properties.Properties
            };

            this.Setup(new object[0], entityMetadata);

            this.testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.AtLeastOnce);
            Assert.True(this.clues.Count > 0);
        }

        [Theory(Skip = "TODO Currently failing")]
        [InlineData("Non-Existing Organization", "https://www.itsdummy.companysite/")]
        [InlineData("Hanging gardens", null)]
        [Trait("Category", "slow")]
        public void TestNoClueProduction(string name, [CanBeNull] string website)
        {
            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName, name);

            if (website != null)
                properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, website);

            IEntityMetadata entityMetadata = new EntityMetadataPart() {
                Name = name,
                EntityType = EntityType.Organization,
                Properties = properties.Properties
            };

            this.Setup(new object[0], entityMetadata);

            this.testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.Never);
            Assert.True(this.clues.Count == 0);
        }
    }
}

