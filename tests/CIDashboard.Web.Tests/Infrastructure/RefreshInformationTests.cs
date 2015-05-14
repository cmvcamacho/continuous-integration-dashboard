using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Entities;
using CIDashboard.Web.Infrastructure;
using CIDashboard.Web.Infrastructure.Interfaces;
using CIDashboard.Web.MappingProfiles;
using FakeItEasy;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace CIDashboard.Web.Tests.Infrastructure
{
    [TestFixture]
    public class RefreshInformationTests
    {
        private IFixture _fixture;
        private IConnectionsManager _connectionsManager;
        private IInformationQuery _infoQuery;

        [SetUp]
        public void Setup()
        {
            Mapper.Configuration.AddProfile<ViewModelProfilers>();

            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            _connectionsManager = A.Fake<IConnectionsManager>();
            _infoQuery = A.Fake<IInformationQuery>();
        }

        [Test]
        public async Task SendRefreshBuildResults_WithNullConnectionId_QueriesGetLastBuildResultForAllBuilds()
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

            var refreshInfo = new RefreshInformation
            {
                ConnectionsManager = _connectionsManager,
                InfoQuery = _infoQuery
            };

            var buildsPerConnId = new ConcurrentDictionary<string, List<string>>();
            buildsPerConnId.AddOrUpdate(_fixture.Create<string>(), buildsIds1, (oldkey, oldvalue) => buildsIds1);
            buildsPerConnId.AddOrUpdate(_fixture.Create<string>(), buildsIds2, (oldkey, oldvalue) => buildsIds1);
            A.CallTo(() => _connectionsManager.BuildsPerConnId)
                .Returns(buildsPerConnId);

            var buildsToBeRefreshed = new ConcurrentDictionary<string, string>();
            Parallel.ForEach(buildsIds, build => buildsToBeRefreshed.TryAdd(build, build));
            A.CallTo(() => _connectionsManager.BuildsToBeRefreshed)
                .Returns(buildsToBeRefreshed);

            await refreshInfo.SendRefreshBuildResults(null);

            foreach (var buildsId in buildsIds)
            {
                A.CallTo(() => _infoQuery.GetLastBuildResult(buildsId))
                    .MustHaveHappened();
            }
        }

        [Test]
        public async Task SendRefreshBuildResults_WithConnectionId_QueriesGetLastBuildResult_OnlyForSpecificConnectionIdBuilds()
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

            var refreshInfo = new RefreshInformation
            {
                ConnectionsManager = _connectionsManager,
                InfoQuery = _infoQuery
            };

            var buildsPerConnId = new ConcurrentDictionary<string, List<string>>();
            buildsPerConnId.AddOrUpdate(connectionId, buildsIds1, (oldkey, oldvalue) => buildsIds1);
            buildsPerConnId.AddOrUpdate(_fixture.Create<string>(), buildsIds2, (oldkey, oldvalue) => buildsIds1);
            A.CallTo(() => _connectionsManager.BuildsPerConnId)
                .Returns(buildsPerConnId);

            var buildsToBeRefreshed = new ConcurrentDictionary<string, string>();
            Parallel.ForEach(buildsIds, build => buildsToBeRefreshed.TryAdd(build, build));
            A.CallTo(() => _connectionsManager.BuildsToBeRefreshed)
                .Returns(buildsToBeRefreshed);

            await refreshInfo.SendRefreshBuildResults(connectionId);

            foreach (var buildsId in buildsIds1)
            {
                A.CallTo(() => _infoQuery.GetLastBuildResult(buildsId))
                    .MustHaveHappened();
            }

            foreach (var buildsId in buildsIds2)
            {
                A.CallTo(() => _infoQuery.GetLastBuildResult(buildsId))
                    .MustNotHaveHappened();
            }
        }
    }
}
