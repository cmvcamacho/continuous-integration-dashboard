using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
using CIDashboard.Domain.Entities;
using CIDashboard.Domain.Services;
using CIDashboard.Web.Application;
using CIDashboard.Web.MappingProfiles;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace CIDashboard.Web.Tests.Application
{
    [TestFixture]
    public class InformationQueryTests
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
            this._fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            this._fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _ciDashboardService = A.Fake<ICiDashboardService>();
            _ciServerService = A.Fake<ICiServerService>();
        }

        [Test]
        public async Task GetUserProjectsAndBuildConfigs_QueriesForUserProjects_AndReturnsMappedValues()
        {
            var username = _fixture.Create<string>();

            var infoQuery = new InformationQuery { CiDashboardService = this._ciDashboardService };

            var projects = _fixture
                .CreateMany<Project>()
                .ToList();

            A.CallTo(() => _ciDashboardService.GetProjects(username))
                .Returns(projects);
            var mappedProjects = Mapper.Map<IEnumerable<Project>, IEnumerable<Models.Project>>(projects);

            var result = await infoQuery.GetUserProjectsAndBuildConfigs(username);

            A.CallTo(() => _ciDashboardService.GetProjects(username)).MustHaveHappened();

            result.ShouldBeEquivalentTo(mappedProjects);
        }

        [Test]
        public async Task GetAllProjectBuildConfigs_QueriesForAllBuildConfigs_AndReturnsMappedValues()
        {
            var infoQuery = new InformationQuery { CiServerService = this._ciServerService };

            var builds = _fixture
                .CreateMany<CiBuildConfig>()
                .ToList();
            
            A.CallTo(() => _ciServerService
                .GetAllBuildConfigs())
                .Returns(builds);
            var mappedBuilds = Mapper.Map<IEnumerable<CiBuildConfig>, IEnumerable<Models.BuildConfig>>(builds);

            var result = await infoQuery.GetAllProjectBuildConfigs();

            A.CallTo(() => _ciServerService
                .GetAllBuildConfigs())
                .MustHaveHappened();

            result.ShouldBeEquivalentTo(mappedBuilds);
        }

        [Test]
        public async Task GetLastBuildResult_QueriesForLastBuildResult_AndReturnsMappedValues()
        {
            var infoQuery = new InformationQuery { CiServerService = this._ciServerService };

            var build = _fixture
                .Build<CiBuildResult>()
                .With(p => p.Id, _fixture.Create<int>().ToString())
                .Create();

            A.CallTo(() => _ciServerService
                .LastBuildResult(build.BuildId))
                .Returns(build);
            var mappedBuild = Mapper.Map<CiBuildResult, Models.Build>(build);

            var result = await infoQuery.GetLastBuildResult(build.BuildId);

            A.CallTo(() => _ciServerService
                .LastBuildResult(build.BuildId))
                .MustHaveHappened();

            result.ShouldBeEquivalentTo(mappedBuild);
        }
    }
}
