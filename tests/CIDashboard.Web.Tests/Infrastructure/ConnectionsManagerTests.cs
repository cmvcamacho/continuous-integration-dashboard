using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Web.Infrastructure;
using CIDashboard.Web.MappingProfiles;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace CIDashboard.Web.Tests.Infrastructure
{
    [TestFixture]
    public class ConnectionsManagerTests
    {
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            Mapper.Configuration.AddProfile<ViewModelProfilers>();

            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            // clean any existing info in static dictionaties
            var connManager = new ConnectionsManager();
            var connIds = connManager.BuildsPerConnId.Keys;
            foreach(var connId in connIds)
            {
                connManager.RemoveAllBuildConfigs(connId).Wait(); 
            }
        }

        [Test]
        public async Task AddBuildConfigs_AddsConnectionInfoStaticDictionaries()
        {
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();
            var buildsIds = builds.Select(b => b.CiExternalId)
                .ToList();

            var connManager = new ConnectionsManager();
            await connManager.AddBuildConfigs(connectionId, builds);

            connManager.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            connManager.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
            connManager.BuildsToBeRefreshed.Should().ContainKeys(buildsIds.ToArray());
        }

        [Test]
        public async Task AddBuildConfigs_DontDuplicateBuildsToBeRefreshed()
        {
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();
            var buildsIds = builds.Select(b => b.CiExternalId)
                .ToList();

            var connManager = new ConnectionsManager();
            await connManager.AddBuildConfigs(_fixture.Create<string>(), new[] { builds.First() });

            await connManager.AddBuildConfigs(connectionId, builds);

            connManager.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            connManager.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
            connManager.BuildsToBeRefreshed.Should().ContainKeys(buildsIds.ToArray());
        }

        [Test]
        public async Task AddBuildConfigs_WhenConnectionAlreadyExists_RefreshsBuildConfigs()
        {
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();
            var buildsIds = builds.Select(b => b.CiExternalId)
                .ToList();
            var olderBuilds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();
            var olderBuildsIds = olderBuilds
                .Select(b => b.CiExternalId)
                .ToList();

            var connManager = new ConnectionsManager();
            await connManager.AddBuildConfigs(connectionId, olderBuilds);
            connManager.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            connManager.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(olderBuildsIds);

            await connManager.AddBuildConfigs(connectionId, builds);

            connManager.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            connManager.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
        }

        [Test]
        public async Task UpdateBuildConfigs_DontDuplicateBuildsToBeRefreshed()
        {
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();

            var otherBuilds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();
            otherBuilds.Add(builds.First());

            var buildsIds = builds.Select(b => b.CiExternalId).ToList();
            buildsIds.AddRange(otherBuilds.Select(b => b.CiExternalId));
            buildsIds = buildsIds.Distinct().ToList();

            var connManager = new ConnectionsManager();
            await connManager.AddBuildConfigs(connectionId, builds);

            await connManager.UpdateBuildConfigs(connectionId, otherBuilds);

            connManager.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            connManager.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
            connManager.BuildsToBeRefreshed.Should().ContainKeys(buildsIds.ToArray());
        }

        [Test]
        public async Task RemoveAllBuildConfigs_ShouldRemoveAllBuildsForConnectionId()
        {
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany();

            var connManager = new ConnectionsManager();
            await connManager.AddBuildConfigs(connectionId, builds);

            connManager.BuildsPerConnId.Keys.Should()
                .Contain(connectionId);

            await connManager.RemoveAllBuildConfigs(connectionId);

            connManager.BuildsPerConnId.Keys.Should().BeEmpty();
            connManager.BuildsToBeRefreshed.Keys.Should().BeEmpty();
        }

        [Test]
        public async Task RemoveAllBuildConfigs_WhenOtherConnectionIdsAreUsingIt_ShouldNotRemoveTheBuild()
        {
            var duplicateBuild = _fixture.Create<Models.BuildConfig>();
            var duplicateBuildId = duplicateBuild.CiExternalId;

            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();
            builds.Add(duplicateBuild);

            var otherConnectionId = _fixture.Create<string>();
            var otherBuilds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();
            otherBuilds.Add(duplicateBuild);
            var otherBuildsIds = otherBuilds
                .Select(b => b.CiExternalId)
                .ToList();

            var connManager = new ConnectionsManager();
            await connManager.AddBuildConfigs(connectionId, builds);
            await connManager.AddBuildConfigs(otherConnectionId, otherBuilds);

            await connManager.RemoveAllBuildConfigs(connectionId);

            connManager.BuildsPerConnId.Keys.Count.Should().Be(1);
            connManager.BuildsToBeRefreshed.Should().ContainKeys(otherBuildsIds.ToArray());
            connManager.BuildsToBeRefreshed.Should().ContainKey(duplicateBuildId);
        }

        [Test]
        public async Task RemoveBuildConfig_ShouldRemoveTheBuild()
        {
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();

            var connManager = new ConnectionsManager();
            await connManager.AddBuildConfigs(connectionId, builds);

            connManager.BuildsPerConnId.Keys.Should()
                .Contain(connectionId);

            await connManager.RemoveBuildConfig(connectionId, builds.First());

            connManager.BuildsPerConnId.Keys.Should().Contain(connectionId);
            connManager.BuildsPerConnId[connectionId].Count.Should()
                .Be(builds.Count() - 1);

            connManager.BuildsToBeRefreshed.Keys.Count.Should()
                .Be(builds.Count() - 1);
        }

        [Test]
        public async Task RemoveBuildConfig_WhenOtherConnectionIdsAreUsingIt_ShouldNotRemoveTheBuild()
        {
            var duplicateBuild = _fixture.Create<Models.BuildConfig>();

            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();
            builds.Add(duplicateBuild);

            var otherConnectionId = _fixture.Create<string>();
            var otherBuilds = _fixture
                .Build<Models.BuildConfig>()
                .CreateMany()
                .ToList();
            otherBuilds.Add(duplicateBuild);

            var connManager = new ConnectionsManager();
            await connManager.AddBuildConfigs(connectionId, builds);
            await connManager.AddBuildConfigs(otherConnectionId, otherBuilds);

            var removeBuild = builds.First();
            await connManager.RemoveBuildConfig(connectionId, removeBuild);

            connManager.BuildsPerConnId.Keys.Count.Should().Be(2);
            connManager.BuildsToBeRefreshed.Should().NotContainKey(removeBuild.CiExternalId);
        }
    }
}
