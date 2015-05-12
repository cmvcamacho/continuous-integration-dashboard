using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
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
    public class CommandControllerTests
    {
        private IFixture _fixture;
        private ICiDashboardService _ciDashboardService;

        [SetUp]
        public void Setup()
        {
            Mapper.Configuration.AddProfile<ViewModelProfilers>();

            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            _ciDashboardService = A.Fake<ICiDashboardService>();
        }

        [Test]
        public async Task AddNewProjectCallsAddProject()
        {
            var username = _fixture.Create<string>();
            var project = new Models.Project { Name = _fixture.Create<string>() };
            
            var commandController = new CommandController();
            commandController.CiDashboardService = _ciDashboardService;

            var result = await commandController.AddNewProject(username, project);

            A.CallTo(() => _ciDashboardService.AddProject(username, 
                A<Project>.That.Matches(p => p.Name == project.Name)))
                .MustHaveHappened();

            result.Should()
                .NotBeNull();
        }

        [Test]
        public async Task UpdateProjectNameCallsUpdateProjectName()
        {
            var project = _fixture.Create<Models.Project>();

            var commandController = new CommandController();
            commandController.CiDashboardService = _ciDashboardService;

            var result = await commandController.UpdateProjectName(project.Id, project.Name);

            A.CallTo(() => _ciDashboardService.UpdateProjectName(project.Id, project.Name))
                .MustHaveHappened();

            result.Should()
                .BeTrue();
        }

        [Test]
        public async Task RemoveProjectCallsRemoveProject()
        {
            var projectId = _fixture.Create<int>();

            var commandController = new CommandController();
            commandController.CiDashboardService = _ciDashboardService;

            var result = await commandController.RemoveProject(projectId);

            A.CallTo(() => _ciDashboardService.RemoveProject(projectId))
                .MustHaveHappened();

            result.Should()
                .BeTrue();
        }

        [Test]
        public async Task AddBuildToProjectCallsAddBuildToProject()
        {
            var projectId = _fixture.Create<int>();
            var build = _fixture.Create<Models.BuildConfig>();

            var commandController = new CommandController();
            commandController.CiDashboardService = _ciDashboardService;

            var result = await commandController.AddBuildToProject(projectId, build);

            A.CallTo(() => _ciDashboardService
                .AddBuildConfigToProject(projectId, A<BuildConfig>.That.Matches(p => p.Name == build.Name)))
                .MustHaveHappened();

            result.Should()
                .NotBeNull();
        }
    }
}
