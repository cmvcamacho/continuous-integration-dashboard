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
        public async Task GetAllProjectBuildsReturnsProjectAndBuildTypesCorrectlyMapped()
        {
            var projects = _fixture
                .Build<Project>()
                .CreateMany();

            A.CallTo(() => _teamcityClient.Projects.All())
                .Returns(projects.ToList());

            var teamCityService = new TeamCityService(_teamcityClient);
            var result = await teamCityService.GetAllProjectBuilds();

            var expectedResult = projects.Select(
                p => new CiProject
                {
                    CiSource = CiSource.TeamCity,
                    Id = p.Id,
                    Name = p.Name,
                    Url = p.WebUrl,
                    Builds = p.BuildTypes.BuildType.Select(
                        b => new CiBuild
                        {
                            CiSource = CiSource.TeamCity,
                            Id = b.Id,
                            Name = b.Name,
                            Url = b.WebUrl
                        })
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

            var teamCityService = new TeamCityService(_teamcityClient);
            var result = await teamCityService.LastBuildResult(buildId);

            var expectedResult = new CiBuildResult
            {
                CiSource = CiSource.TeamCity,
                Id = build.Id,
                BuildId = build.BuildConfig.Id,
                BuildName = build.BuildConfig.Name,
                Url = build.WebUrl,
                FinishDate = build.FinishDate,
                StartDate = build.StartDate,
                Version = build.Number,
                Status = resultStatus
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

            A.CallTo(() => 
                _teamcityClient.Builds.ByBuildLocator(A<BuildLocator>.Ignored))
                .Returns(_fixture.Build<Build>().CreateMany().ToList());

            var teamCityService = new TeamCityService(_teamcityClient);
            var result = await teamCityService.LastBuildResult(buildId);

            var expectedResult = new CiBuildResult
            {
                CiSource = CiSource.TeamCity,
                Id = build.Id,
                BuildId = build.BuildConfig.Id,
                BuildName = build.BuildConfig.Name,
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
