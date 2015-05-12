using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Domain.Entities;
using CIDashboard.Domain.MappingProfiles;
using CIDashboard.Domain.Services;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;
using TeamCitySharp;
using TeamCitySharp.DomainEntities;
using TeamCitySharp.Locators;

namespace CIDashboard.Domain.Tests.Services
{
    [TestFixture]
    public class TeamCityServiceTests
    {
        private IFixture _fixture;
        private ITeamCityClient _teamcityClient;

        [SetUp]
        public void Setup()
        {
            this._fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());
            this._fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            this._fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
            Mapper.Initialize(cfg => cfg.AddProfile<TeamCityProfiler>());

            _teamcityClient = A.Fake<ITeamCityClient>();
        }

        [Test]
        public void ParameterlessConstructorLoginsAsGuest()
        {
            new TeamCityService(this._teamcityClient);
            A.CallTo(() => _teamcityClient.ConnectAsGuest()).MustHaveHappened();
        }

        [Test]
        public void ConstructorWithParametersLoginsWithUserPwd()
        {
            new TeamCityService(this._teamcityClient, "user", "pwd");
            A.CallTo(() => _teamcityClient.Connect("user", "pwd")).MustHaveHappened();
        }

        [Test]
        public async Task GetAllBuildConfigsReturnsProjectAndBuildTypesCorrectlyMapped()
        {
            var buildConfigs = _fixture
                .Build<BuildConfig>()
                .CreateMany();

            A.CallTo(() => _teamcityClient.BuildConfigs.All())
                .Returns(buildConfigs.ToList());

            var teamCityService = new TeamCityService(_teamcityClient);
            var result = await teamCityService.GetAllBuildConfigs();

            var expectedResult = buildConfigs.Select(
                b => new CiBuildConfig()
                {
                    CiSource = CiSource.TeamCity,
                    Id = b.Id,
                    Name = b.Name,
                    Url = b.WebUrl,
                    ProjectName = b.ProjectName
                });

            result.ShouldBeEquivalentTo(expectedResult);     
        }

        [TestCase("SUCCESS", CiBuildResultStatus.Success)]
        [TestCase("FAILURE", CiBuildResultStatus.Failure)]
        public async Task LastBuildResultReturnsInfoCorrectlyMapped(string status, CiBuildResultStatus resultStatus)
        {
            var buildId = _fixture.Create<string>();
            var build = _fixture
                .Build<Build>()
                .With(b => b.Status, status)
                .Create();

            A.CallTo(() => _teamcityClient.Builds.LastBuildByBuildConfigId(buildId))
                .Returns(build);

            A.CallTo(() => _teamcityClient.Builds.ByBuildId(build.Id))
                .Returns(build);

            var teamCityService = new TeamCityService(_teamcityClient);
            var result = await teamCityService.LastBuildResult(buildId);

            var expectedResult = new CiBuildResult
            {
                CiSource = CiSource.TeamCity,
                Id = build.Id,
                BuildId = build.BuildType.Id,
                BuildName = build.BuildType.Name,
                Url = build.WebUrl,
                FinishDate = build.FinishDate,
                StartDate = build.StartDate,
                Version = build.Number,
                Status = resultStatus
            };

            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task LastBuildResultReturnsStatisticsCorrectlyMapped()
        {
            var buildId = _fixture.Create<string>();
            var build = _fixture
                .Build<Build>()
                .With(b => b.Status, "SUCCESS")
                .Create();

            A.CallTo(() => _teamcityClient.Builds.LastBuildByBuildConfigId(buildId))
                .Returns(build);

            A.CallTo(() => _teamcityClient.Builds.ByBuildId(build.Id))
                .Returns(build);

            var stats = new List<Property>
            {
                new Property{Name = "PassedTestCount", Value = "1"},
                new Property{Name = "FailedTestCount", Value = "2"},
                new Property{Name = "IgnoredTestCount", Value = "3"},
                new Property{Name = "CodeCoverageAbsSCovered", Value = "4"},
                new Property{Name = "CodeCoverageAbsSTotal", Value = "5"},
            };
            A.CallTo(() => _teamcityClient.Statistics.GetByBuildId(build.Id))
                .Returns(stats);

            var teamCityService = new TeamCityService(_teamcityClient);
            var result = await teamCityService.LastBuildResult(buildId);

            var expectedResult = new CiBuildResult
            {
                CiSource = CiSource.TeamCity,
                Id = build.Id,
                BuildId = build.BuildType.Id,
                BuildName = build.BuildType.Name,
                Url = build.WebUrl,
                FinishDate = build.FinishDate,
                StartDate = build.StartDate,
                Version = build.Number,
                Status = CiBuildResultStatus.Success,
                NumberTestPassed = 1,
                NumberTestFailed = 2,
                NumberTestIgnored = 3,
                NumberStatementsCovered = 4,
                NumberStatementsTotal = 5
            };

            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task LastBuildResultReturnsRunningBuildWhenBuildIsRunning()
        {
            var buildId = _fixture.Create<string>();
            var build = _fixture
                .Build<Build>()
                .Create();

            A.CallTo(() => _teamcityClient.Builds.LastBuildByBuildConfigId(buildId))
                .Returns(build);

            A.CallTo(() => _teamcityClient.Builds.ByBuildId(build.Id))
                .Returns(build);

            A.CallTo(() => 
                _teamcityClient.Builds.ByBuildLocator(A<BuildLocator>.Ignored))
                .Returns(_fixture.Build<Build>().CreateMany().ToList());

            var teamCityService = new TeamCityService(_teamcityClient);
            var result = await teamCityService.LastBuildResult(buildId);

            var expectedResult = new CiBuildResult
            {
                CiSource = CiSource.TeamCity,
                Id = build.Id,
                BuildId = build.BuildType.Id,
                BuildName = build.BuildType.Name,
                Url = build.WebUrl,
                FinishDate = build.FinishDate,
                StartDate = build.StartDate,
                Version = build.Number,
                Status = CiBuildResultStatus.Running
            };

            result.ShouldBeEquivalentTo(expectedResult);
        }

    }
}
