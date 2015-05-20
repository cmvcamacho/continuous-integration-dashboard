using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
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
    public class CommandProcessorTests
    {
        private IFixture _fixture;
        private ICiDashboardService ciDashboardService;

        [SetUp]
        public void Setup()
        {
            Mapper.Configuration.AddProfile<ViewModelProfilers>();

            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            this.ciDashboardService = A.Fake<ICiDashboardService>();
        }

        [Test]
        public async Task AddNewProject_ReturnsCreatedProject()
        {
            var username = _fixture.Create<string>();
            var project = new Models.Project { Name = _fixture.Create<string>() };

            var commandController = new CommandProcessor();
            commandController.CiDashboardService = this.ciDashboardService;

            var result = await commandController.AddNewProject(username, project);

            A.CallTo(() => this.ciDashboardService.AddProject(username, 
                A<Project>.That.Matches(p => p.Name == project.Name)))
                .MustHaveHappened();

            result.Should()
                .NotBeNull()
                .And.BeOfType<Models.Project>();
        }

        [Test]
        public async Task UpdateProjectName_ReturnsTrueOnSuccess()
        {
            var project = _fixture.Create<Models.Project>();

            var commandController = new CommandProcessor();
            commandController.CiDashboardService = this.ciDashboardService;

            var result = await commandController.UpdateProjectName(project.Id, project.Name);

            A.CallTo(() => this.ciDashboardService.UpdateProjectName(project.Id, project.Name))
                .MustHaveHappened();

            result.Should()
                .BeTrue();
        }

        [Test]
        public async Task UpdateProjectOrder_ReturnsTrueOnSuccess()
        {
            var project = _fixture.Create<Models.Project>();

            var commandController = new CommandProcessor();
            commandController.CiDashboardService = this.ciDashboardService;

            var newPosition = _fixture.Create<int>();
            var result = await commandController.UpdateProjectOrder(project.Id, newPosition);

            A.CallTo(() => this.ciDashboardService.UpdateProjectOrder(project.Id, newPosition))
                .MustHaveHappened();

            result.Should()
                .BeTrue();
        }

        [Test]
        public async Task RemoveProject_ReturnsRemovedProject()
        {
            var projectId = _fixture.Create<int>();

            var commandController = new CommandProcessor();
            commandController.CiDashboardService = this.ciDashboardService;

            var result = await commandController.RemoveProject(projectId);

            A.CallTo(() => this.ciDashboardService.RemoveProject(projectId))
                .MustHaveHappened();

            result.Should()
                .NotBeNull()
                .And.BeOfType<Models.Project>();
        }

        [Test]
        public async Task AddBuildToProject_ReturnsCreatedBuild()
        {
            var projectId = _fixture.Create<int>();
            var build = _fixture.Create<Models.BuildConfig>();

            var commandController = new CommandProcessor();
            commandController.CiDashboardService = this.ciDashboardService;

            var result = await commandController.AddBuildConfigToProject(projectId, build);

            A.CallTo(() => this.ciDashboardService
                .AddBuildConfigToProject(projectId, A<BuildConfig>.That.Matches(p => p.Name == build.Name)))
                .MustHaveHappened();

            result.Should()
                .NotBeNull()
                .And.BeOfType<Models.BuildConfig>();
        }

        [Test]
        public async Task RemoveBuildConfig_ReturnsRemovedBuildConfig()
        {
            var buidlId = _fixture.Create<int>();

            var commandController = new CommandProcessor();
            commandController.CiDashboardService = this.ciDashboardService;

            var result = await commandController.RemoveBuildConfig(buidlId);

            A.CallTo(() => this.ciDashboardService.RemoveBuildConfig(buidlId))
                .MustHaveHappened();

            result.Should()
                .NotBeNull()
                .And.BeOfType<Models.BuildConfig>();
        }

        [Test]
        public async Task UpdateBuildConfigNameAndExternalId_ReturnsTrueOnSuccess()
        {
            var build = _fixture.Create<Models.BuildConfig>();

            var commandController = new CommandProcessor();
            commandController.CiDashboardService = this.ciDashboardService;

            var result = await commandController.UpdateBuildConfigExternalId(build.Id, build.Name, build.CiExternalId);

            A.CallTo(() => this.ciDashboardService.UpdateBuildConfigExternalId(build.Id, build.Name, build.CiExternalId))
                .MustHaveHappened();

            result.Should()
                .BeTrue();
        }

        [Test]
        public async Task UpdateBuildConfigOrder_ReturnsTrueOnSuccess()
        {
            var build = _fixture.Create<Models.BuildConfig>();

            var commandController = new CommandProcessor();
            commandController.CiDashboardService = this.ciDashboardService;

            var newPosition = _fixture.Create<int>();
            var result = await commandController.UpdateBuildConfigOrder(build.Id, newPosition);

            A.CallTo(() => this.ciDashboardService.UpdateBuildConfigOrder(build.Id, newPosition))
                .MustHaveHappened();

            result.Should()
                .BeTrue();
        }
    }
}
