using System.Security.Principal;
using System.Threading.Tasks;
using CIDashboard.Web.Hubs;
using CIDashboard.Web.Infrastructure;
using CIDashboard.Web.Models;
using FakeItEasy;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace CIDashboard.Web.Tests.Hubs
{
    [TestFixture]
    public class CiDashboardHubTests
    {
        private IPrincipal _principal;
        private HubCallerContext _context;
        private IQueryController queryController;
        private ICommandController commandController;
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            this.queryController = A.Fake<IQueryController>();
            this.commandController = A.Fake<ICommandController>();
            _principal = A.Fake<IPrincipal>();
            _context = A.Fake<HubCallerContext>();
        }

        [Test]
        public async Task OnConnectCallsAddBuildsOfRefreshInfoService()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var hub = new CiDashboardHub(this.queryController, this.commandController) { Context = _context };

            await hub.OnConnected();

            A.CallTo(() => this.queryController.AddBuilds(username, connectionId))
                .MustHaveHappened();
        }

        [Test]
        public async Task OnReconnectedCallsAddBuildsOfRefreshInfoService()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var hub = new CiDashboardHub(this.queryController, this.commandController) { Context = _context };

            await hub.OnReconnected();

            A.CallTo(() => this.queryController.AddBuilds(username, connectionId))
                .MustHaveHappened();
        }

        [Test]
        public async Task OnDisconnectedCallsRemoveBuildsOfRefreshInfoService()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var hub = new CiDashboardHub(this.queryController, this.commandController) { Context = _context };

            await hub.OnDisconnected(true);

            A.CallTo(() => this.queryController.RemoveBuilds(connectionId))
                .MustHaveHappened();
        }

        [Test]
        public async Task RequestRefreshCallsRefreshBuildsForConnectionIdOnly()
        {
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);

            var hub = new CiDashboardHub(this.queryController, this.commandController) { Context = _context };

            await hub.RequestRefresh();

            A.CallTo(() => this.queryController.RefreshBuilds(connectionId))
                .MustHaveHappened();
        }

        [Test]
        public async Task RequestRefreshCallsRequestAllProjectBuildsForConnectionIdOnly()
        {
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);

            var hub = new CiDashboardHub(this.queryController, this.commandController) { Context = _context };

            await hub.RequestAllProjectBuilds();

            A.CallTo(() => this.queryController.RequestAllProjectBuilds(connectionId))
                .MustHaveHappened();
        }

        [Test]
        public async Task AddNewProjectCallsCommandControllerAddNewProjectAndUpdatesUI()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var project = _fixture.Create<Project>();
            var resultProject = _fixture.Create<Project>();

            A.CallTo(() => this.commandController.AddNewProject(username, project))
                .Returns(resultProject);

            var hub = new CiDashboardHub(this.queryController, this.commandController) { Context = _context };
            await hub.AddNewProject(project);

            A.CallTo(() => this.commandController.AddNewProject(username, project))
                .MustHaveHappened();

            A.CallTo(() => this.queryController.UpdateProject(connectionId, project.Id, resultProject))
                .MustHaveHappened();
        }

        [Test]
        public async Task UpdateProjectNameCallsCommandControllerUpdateProjectNameAndUpdatesUI()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var project = _fixture.Create<Project>();

            A.CallTo(() => this.commandController.UpdateProjectName(project.Id, project.Name))
                .Returns(true);

            var hub = new CiDashboardHub(this.queryController, this.commandController) { Context = _context };
            await hub.UpdateProjectName(project.Id, project.Name);

            A.CallTo(() => this.commandController.UpdateProjectName(project.Id, project.Name))
                .MustHaveHappened();

            A.CallTo(() => this.queryController.SendMessage(connectionId, string.Format("Project {0} updated.", project.Name)))
                .MustHaveHappened();
        }


        [Test]
        public async Task RemoveProjectCallsCommandControllerRemoveProjectAndUpdatesUI()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var project = _fixture.Create<Project>();

            A.CallTo(() => this.commandController.RemoveProject(project.Id))
                .Returns(true);

            var hub = new CiDashboardHub(this.queryController, this.commandController) { Context = _context };
            await hub.RemoveProject(project.Id);

            A.CallTo(() => this.commandController.RemoveProject(project.Id))
                .MustHaveHappened();

            A.CallTo(() => this.queryController.SendMessage(connectionId, "Project removed."))
                .MustHaveHappened();
        }

        [Test]
        public async Task AddBuildToProjectCallsCommandControllerAddBuildToProjectAndUpdatesUI()
        {
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);

            var project = _fixture.Create<Project>();

            var build = _fixture.Create<Build>();
            var newBuild = _fixture.Create<Build>();
            A.CallTo(() => this.commandController.AddBuildToProject(project.Id, build))
                .Returns(newBuild);

            var hub = new CiDashboardHub(this.queryController, this.commandController) { Context = _context };
            await hub.AddBuildToProject(project.Id, build);

            A.CallTo(() => this.commandController.AddBuildToProject(project.Id, build))
                .MustHaveHappened();

            A.CallTo(() => this.queryController.UpdateBuild(connectionId, build.Id, newBuild))
                .MustHaveHappened();
        }
    }
}
