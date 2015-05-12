using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
using CIDashboard.Domain.Services;
using CIDashboard.Web.Infrastructure;
using CIDashboard.Web.MappingProfiles;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace CIDashboard.Web.Tests.Infrastructure
{
    [TestFixture]
    public class QueryControllerTests
    {
        private IFixture _fixture;
        private ICiDashboardService _ciDashboardService;
        private ICiServerService _ciServerService;

        [SetUp]
        public void Setup()
        {
            Mapper.Configuration.AddProfile<ViewModelProfilers>();

            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            _ciDashboardService = A.Fake<ICiDashboardService>();
            _ciServerService = A.Fake<ICiServerService>();

            QueryController.BuildsPerConnId.Clear();
            QueryController.BuildsToBeRefreshed.Clear();
        }

        [Test]
        public async Task SendUserBuildConfigsToUiQueriesForProjectsWhenUserIsNew()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var queryController = new QueryController();
            queryController.CiDashboardService = _ciDashboardService;

            await queryController.SendUserProjectsAndBuildConfigs(username, connectionId);

            A.CallTo(() => _ciDashboardService.GetProjects(username)).MustHaveHappened();

            QueryController.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
        }

        [Test]
        public async Task SendUserBuildConfigsToUiAddsConnectionInfoStaticDictionaries()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany();
            var projects = _fixture
                .Build<Project>()
                .With(p => p.BuildConfigs, builds.ToList())
                .Create();
            var buildsIds = builds.Select(b => b.CiExternalId)
                .ToList();

            A.CallTo(() => _ciDashboardService.GetProjects(username))
                .Returns(new[] { projects });
            
            var queryController = new QueryController();
            queryController.CiDashboardService = _ciDashboardService;
            await queryController.SendUserProjectsAndBuildConfigs(username, connectionId);

            QueryController.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            QueryController.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
            QueryController.BuildsToBeRefreshed.Should().ContainKeys(buildsIds.ToArray());
        }

        [Test]
        public async Task SendUserBuildConfigsToUiDontDuplicateBuildsToBeRefreshed()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany();
            var projects = _fixture
                .Build<Project>()
                .With(p => p.BuildConfigs, builds.ToList())
                .Create();
            var buildsIds = builds.Select(b => b.CiExternalId)
                .ToList();

            A.CallTo(() => _ciDashboardService.GetProjects(username))
                .Returns(new[] { projects });

            var queryController = new QueryController();
            queryController.CiDashboardService = _ciDashboardService;
            QueryController.BuildsToBeRefreshed.TryAdd(buildsIds.First(), buildsIds.First());
            QueryController.BuildsToBeRefreshed.TryAdd(_fixture.Create<string>(), _fixture.Create<string>());

            await queryController.SendUserProjectsAndBuildConfigs(username, connectionId);

            QueryController.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            QueryController.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
            QueryController.BuildsToBeRefreshed.Should().ContainKeys(buildsIds.ToArray());
            QueryController.BuildsToBeRefreshed.Keys.Count.ShouldBeEquivalentTo(buildsIds.Count + 1);
        }

        [Test]
        public async Task SendUserBuildConfigsToUiRefreshsProjectsWhenUserAlreadyConnected()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany();
            var projects = _fixture
                .Build<Project>()
                .With(p => p.BuildConfigs, builds.ToList())
                .Create();
            var buildsIds = builds.Select(b => b.CiExternalId)
                .ToList();
            var olderBuildsIds = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany()
                .Select(b => b.CiExternalId)
                .ToList();

            A.CallTo(() => _ciDashboardService.GetProjects(username))
                .Returns(new[] { projects });

            var queryController = new QueryController();
            queryController.CiDashboardService = _ciDashboardService;
            QueryController.BuildsPerConnId.TryAdd(buildsIds.First(), olderBuildsIds);

            await queryController.SendUserProjectsAndBuildConfigs(username, connectionId);

            QueryController.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            QueryController.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
        }

        [Test]
        public async Task RemoveBuildsRemovesBuildsForConnectionId()
        {
            var connectionId = _fixture.Create<string>();
            var buildsIds = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany()
                .Select(b => b.CiExternalId)
                .ToList();

            var queryController = new QueryController();
            queryController.CiDashboardService = _ciDashboardService;
            QueryController.BuildsPerConnId.TryAdd(connectionId, buildsIds);
            foreach (var buildsId in buildsIds)
            {
                QueryController.BuildsToBeRefreshed.TryAdd(buildsId, buildsId);
            }
            
            await queryController.RemoveUserBuildsConfigs(connectionId);

            QueryController.BuildsPerConnId.Keys.Should().BeEmpty();
            QueryController.BuildsToBeRefreshed.Keys.Should().BeEmpty();
        }

        [Test]
        public async Task RemoveBuildsDontRemoveBuildWhenOtherUsersAreUsingIt()
        {
            var duplicateBuildId = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            var buildsIds = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany()
                .Select(b => b.CiExternalId)
                .ToList();
            buildsIds.Add(duplicateBuildId);
            var otherConnectionId = _fixture.Create<string>();
            var otherBuildsIds = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany()
                .Select(b => b.CiExternalId)
                .ToList();
            otherBuildsIds.Add(duplicateBuildId);

            var queryController = new QueryController();
            queryController.CiDashboardService = _ciDashboardService;
            QueryController.BuildsPerConnId.TryAdd(connectionId, buildsIds);
            QueryController.BuildsPerConnId.TryAdd(otherConnectionId, otherBuildsIds);
            foreach (var buildsId in buildsIds)
            {
                QueryController.BuildsToBeRefreshed.TryAdd(buildsId, buildsId);
            }

            foreach (var buildsId in otherBuildsIds)
            {
                QueryController.BuildsToBeRefreshed.TryAdd(buildsId, buildsId);
            }

            await queryController.RemoveUserBuildsConfigs(connectionId);

            QueryController.BuildsPerConnId.Keys.Should().Contain(new[] { otherConnectionId });
            QueryController.BuildsToBeRefreshed.Should().ContainKeys(otherBuildsIds.ToArray());
            QueryController.BuildsToBeRefreshed.Should().ContainKey(duplicateBuildId);
        }

        [Test]
        public async Task SendRefreshBuildResultsWithNullConnectionIdCallLastBuildResultForAllBuilds()
        {
            var buildsProj1 = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany();
             var buildsIds1 = buildsProj1.Select(b => b.CiExternalId)
                 .ToList();
             var buildsProj2 = _fixture
                  .Build<BuildConfig>()
                  .Without(p => p.Project)
                  .CreateMany();
             var buildsIds2 = buildsProj2.Select(b => b.CiExternalId)
                 .ToList();

            var buildsIds = new List<string>();
            buildsIds.AddRange(buildsIds1); 
            buildsIds.AddRange(buildsIds2);

             var queryController = new QueryController();
             queryController.CiServerService = _ciServerService;
             QueryController.BuildsPerConnId.AddOrUpdate(_fixture.Create<string>(), buildsIds1, (oldkey, oldvalue) => buildsIds1);
             QueryController.BuildsPerConnId.AddOrUpdate(_fixture.Create<string>(), buildsIds2, (oldkey, oldvalue) => buildsIds1);
             Parallel.ForEach(buildsIds,
                 build => QueryController.BuildsToBeRefreshed.TryAdd(build, build));

            await queryController.SendRefreshBuildResults(null);

            foreach(var buildsId in buildsIds)
            {
                A.CallTo(() => _ciServerService.LastBuildResult(buildsId))
                    .MustHaveHappened();
            }
        }
        
        [Test]
        public async Task SendRefreshBuildResultsWithConnectionIdCallLastBuildResultForSpecificConnectionIdBuilds()
        {
            var buildsProj1 = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany();
            var buildsIds1 = buildsProj1.Select(b => b.CiExternalId)
                .ToList();
            var buildsProj2 = _fixture
                 .Build<BuildConfig>()
                 .Without(p => p.Project)
                 .CreateMany();
            var buildsIds2 = buildsProj2.Select(b => b.CiExternalId)
                .ToList();

            var buildsIds = new List<string>();
            buildsIds.AddRange(buildsIds1);
            buildsIds.AddRange(buildsIds2);

            var connectionId = _fixture.Create<string>();

            var queryController = new QueryController();
            queryController.CiServerService = _ciServerService;
            QueryController.BuildsPerConnId.AddOrUpdate(connectionId, buildsIds1, (oldkey, oldvalue) => buildsIds1);
            QueryController.BuildsPerConnId.AddOrUpdate(_fixture.Create<string>(), buildsIds2, (oldkey, oldvalue) => buildsIds1);
            Parallel.ForEach(buildsIds,
                build => QueryController.BuildsToBeRefreshed.TryAdd(build, build));

            await queryController.SendRefreshBuildResults(connectionId);

            foreach (var buildsId in buildsIds1)
            {
                A.CallTo(() => _ciServerService.LastBuildResult(buildsId))
                    .MustHaveHappened();
            }

            foreach (var buildsId in buildsIds2)
            {
                A.CallTo(() => _ciServerService.LastBuildResult(buildsId))
                    .MustNotHaveHappened();
            }
        }

        [Test]
        public async Task RequestAllProjectBuildsWithConnectionIdCallGetAllProjectBuilds()
        {
            var connectionId = _fixture.Create<string>();

            var queryController = new QueryController {CiServerService = _ciServerService};
            await queryController.RequestAllProjectBuilds(connectionId);

            A.CallTo(() => _ciServerService
                .GetAllBuildConfigs())
                .MustHaveHappened();
        }

        [Test]
        public async Task UpdateBuildShouldUpdateBuildsToBeRefreshed()
        {
            var buildsProj = _fixture
                .Build<BuildConfig>()
                .Without(p => p.Project)
                .CreateMany();
            var buildsIds = buildsProj.Select(b => b.CiExternalId)
                .ToList();
            var connectionId = _fixture.Create<string>();

            var queryController = new QueryController();
            queryController.CiServerService = _ciServerService;
            QueryController.BuildsPerConnId.AddOrUpdate(connectionId, buildsIds, (oldkey, oldvalue) => buildsIds);
            Parallel.ForEach(buildsIds,
                build => QueryController.BuildsToBeRefreshed.TryAdd(build, build));

            var newBuild = _fixture.Build<Models.BuildConfig>().Create();
            await queryController.SendUpdatedBuild(connectionId, -1, newBuild);

            QueryController.BuildsPerConnId[connectionId]
                .Should().Contain(newBuild.CiExternalId);

            QueryController.BuildsToBeRefreshed
                .Should().ContainKey(newBuild.CiExternalId);
        }
    }
}
