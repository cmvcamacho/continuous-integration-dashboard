using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
using CIDashboard.Domain.Entities;
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
    public class RefreshInformationTests
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

            RefreshInformation.BuildsPerConnId.Clear();
            RefreshInformation.BuildsToBeRefreshed.Clear();
        }

        [Test]
        public async Task AddBuildsQueriesForProjectsWhenUserIsNew()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();
            refresh.CiDashboardService = _ciDashboardService;

            await refresh.AddBuilds(username, connectionId);

            A.CallTo(() => _ciDashboardService.GetProjects(username)).MustHaveHappened();

            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
        }

        [Test]
        public async Task AddBuildsAddsConnectionInfoStaticDictionaries()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Build>()
                .Without(p => p.Project)
                .CreateMany();
            var projects = _fixture
                .Build<Project>()
                .With(p => p.Builds, builds.ToList())
                .Create();
            var buildsIds = builds.Select(b => b.CiExternalId)
                .ToList();

            A.CallTo(() => _ciDashboardService.GetProjects(username))
                .Returns(new[] { projects });
            
            var refresh = new RefreshInformation();
            refresh.CiDashboardService = _ciDashboardService;
            await refresh.AddBuilds(username, connectionId);

            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            RefreshInformation.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
            RefreshInformation.BuildsToBeRefreshed.Should().ContainKeys(buildsIds.ToArray());
        }

        [Test]
        public async Task AddBuildsDontDuplicateBuildsToBeRefreshed()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Build>()
                .Without(p => p.Project)
                .CreateMany();
            var projects = _fixture
                .Build<Project>()
                .With(p => p.Builds, builds.ToList())
                .Create();
            var buildsIds = builds.Select(b => b.CiExternalId)
                .ToList();

            A.CallTo(() => _ciDashboardService.GetProjects(username))
                .Returns(new[] { projects });

            var refresh = new RefreshInformation();
            refresh.CiDashboardService = _ciDashboardService;
            RefreshInformation.BuildsToBeRefreshed.TryAdd(buildsIds.First(), buildsIds.First());
            RefreshInformation.BuildsToBeRefreshed.TryAdd(_fixture.Create<string>(), _fixture.Create<string>());

            await refresh.AddBuilds(username, connectionId);

            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            RefreshInformation.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
            RefreshInformation.BuildsToBeRefreshed.Should().ContainKeys(buildsIds.ToArray());
            RefreshInformation.BuildsToBeRefreshed.Keys.Count.ShouldBeEquivalentTo(buildsIds.Count + 1);
        }

        [Test]
        public async Task AddBuildsRefreshsProjectsWhenUserAlreadyConnected()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            var builds = _fixture
                .Build<Build>()
                .Without(p => p.Project)
                .CreateMany();
            var projects = _fixture
                .Build<Project>()
                .With(p => p.Builds, builds.ToList())
                .Create();
            var buildsIds = builds.Select(b => b.CiExternalId)
                .ToList();
            var olderBuildsIds = _fixture
                .Build<Build>()
                .Without(p => p.Project)
                .CreateMany()
                .Select(b => b.CiExternalId)
                .ToList();

            A.CallTo(() => _ciDashboardService.GetProjects(username))
                .Returns(new[] { projects });

            var refresh = new RefreshInformation();
            refresh.CiDashboardService = _ciDashboardService;
            RefreshInformation.BuildsPerConnId.TryAdd(buildsIds.First(), olderBuildsIds);

            await refresh.AddBuilds(username, connectionId);

            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            RefreshInformation.BuildsPerConnId[connectionId].ShouldAllBeEquivalentTo(buildsIds);
        }

        [Test]
        public async Task RemoveBuildsRemovesBuildsForConnectionId()
        {
            var connectionId = _fixture.Create<string>();
            var buildsIds = _fixture
                .Build<Build>()
                .Without(p => p.Project)
                .CreateMany()
                .Select(b => b.CiExternalId)
                .ToList();

            var refresh = new RefreshInformation();
            refresh.CiDashboardService = _ciDashboardService;
            RefreshInformation.BuildsPerConnId.TryAdd(connectionId, buildsIds);
            foreach (var buildsId in buildsIds)
            {
                RefreshInformation.BuildsToBeRefreshed.TryAdd(buildsId, buildsId);
            }
            
            await refresh.RemoveBuilds(connectionId);

            RefreshInformation.BuildsPerConnId.Keys.Should().BeEmpty();
            RefreshInformation.BuildsToBeRefreshed.Keys.Should().BeEmpty();
        }

        [Test]
        public async Task RemoveBuildsDontRemoveBuildWhenOtherUsersAreUsingIt()
        {
            var duplicateBuildId = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            var buildsIds = _fixture
                .Build<Build>()
                .Without(p => p.Project)
                .CreateMany()
                .Select(b => b.CiExternalId)
                .ToList();
            buildsIds.Add(duplicateBuildId);
            var otherConnectionId = _fixture.Create<string>();
            var otherBuildsIds = _fixture
                .Build<Build>()
                .Without(p => p.Project)
                .CreateMany()
                .Select(b => b.CiExternalId)
                .ToList();
            otherBuildsIds.Add(duplicateBuildId);

            var refresh = new RefreshInformation();
            refresh.CiDashboardService = _ciDashboardService;
            RefreshInformation.BuildsPerConnId.TryAdd(connectionId, buildsIds);
            RefreshInformation.BuildsPerConnId.TryAdd(otherConnectionId, otherBuildsIds);
            foreach (var buildsId in buildsIds)
            {
                RefreshInformation.BuildsToBeRefreshed.TryAdd(buildsId, buildsId);
            }

            foreach (var buildsId in otherBuildsIds)
            {
                RefreshInformation.BuildsToBeRefreshed.TryAdd(buildsId, buildsId);
            }

            await refresh.RemoveBuilds(connectionId);

            RefreshInformation.BuildsPerConnId.Keys.Should().Contain(new[] { otherConnectionId });
            RefreshInformation.BuildsToBeRefreshed.Should().ContainKeys(otherBuildsIds.ToArray());
            RefreshInformation.BuildsToBeRefreshed.Should().ContainKey(duplicateBuildId);
        }

        [Test]
        public async Task RefreshBuildsWithNullConnectionIdCallLastBuildResultForAllBuilds()
        {
            var buildsProj1 = _fixture
                .Build<Build>()
                .Without(p => p.Project)
                .CreateMany();
             var buildsIds1 = buildsProj1.Select(b => b.CiExternalId)
                 .ToList();
             var buildsProj2 = _fixture
                  .Build<Build>()
                  .Without(p => p.Project)
                  .CreateMany();
             var buildsIds2 = buildsProj2.Select(b => b.CiExternalId)
                 .ToList();

            var buildsIds = new List<string>();
            buildsIds.AddRange(buildsIds1); 
            buildsIds.AddRange(buildsIds2);

             var refresh = new RefreshInformation();
             refresh.CiServerService = _ciServerService;
             RefreshInformation.BuildsPerConnId.AddOrUpdate(_fixture.Create<string>(), buildsIds1, (oldkey, oldvalue) => buildsIds1);
             RefreshInformation.BuildsPerConnId.AddOrUpdate(_fixture.Create<string>(), buildsIds2, (oldkey, oldvalue) => buildsIds1);
             Parallel.ForEach(buildsIds,
                 build => RefreshInformation.BuildsToBeRefreshed.TryAdd(build, build));

            await refresh.RefreshBuilds(null);

            foreach(var buildsId in buildsIds)
            {
                A.CallTo(() => _ciServerService.LastBuildResult(buildsId))
                    .MustHaveHappened();
            }
        }
        
        [Test]
        public async Task RefreshBuildsWithConnectionIdCallLastBuildResultForSpecificConnectionIdBuilds()
        {
            var buildsProj1 = _fixture
                .Build<Build>()
                .Without(p => p.Project)
                .CreateMany();
            var buildsIds1 = buildsProj1.Select(b => b.CiExternalId)
                .ToList();
            var buildsProj2 = _fixture
                 .Build<Build>()
                 .Without(p => p.Project)
                 .CreateMany();
            var buildsIds2 = buildsProj2.Select(b => b.CiExternalId)
                .ToList();

            var buildsIds = new List<string>();
            buildsIds.AddRange(buildsIds1);
            buildsIds.AddRange(buildsIds2);

            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();
            refresh.CiServerService = _ciServerService;
            RefreshInformation.BuildsPerConnId.AddOrUpdate(connectionId, buildsIds1, (oldkey, oldvalue) => buildsIds1);
            RefreshInformation.BuildsPerConnId.AddOrUpdate(_fixture.Create<string>(), buildsIds2, (oldkey, oldvalue) => buildsIds1);
            Parallel.ForEach(buildsIds,
                build => RefreshInformation.BuildsToBeRefreshed.TryAdd(build, build));

            await refresh.RefreshBuilds(connectionId);

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

            var refresh = new RefreshInformation();
            refresh.CiServerService = _ciServerService;
            await refresh.RequestAllProjectBuilds(connectionId);

            A.CallTo(() => _ciServerService
                .GetAllProjectBuilds())
                .MustHaveHappened();
        }
    }
}
