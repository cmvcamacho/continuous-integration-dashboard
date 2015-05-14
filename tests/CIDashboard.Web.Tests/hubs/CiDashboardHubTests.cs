using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using CIDashboard.Web.Hubs;
using CIDashboard.Web.Infrastructure;
using CIDashboard.Web.Infrastructure.Interfaces;
using CIDashboard.Web.Models;
using FakeItEasy;
using FluentAssertions;
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
        private IHubCallerConnectionContext<ICiDashboardHub> _fakeClients;
        private ICiDashboardHub _fakeClient;
        private IFixture _fixture;
        private IConnectionsManager connectionsManager;
        private IInformationQuery infoQuery;
        private ICommandProcessor commandProcessor;
        private IRefreshInformation refreshInfo;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            this.infoQuery = A.Fake<IInformationQuery>();
            this.connectionsManager = A.Fake<IConnectionsManager>();
            this.commandProcessor = A.Fake<ICommandProcessor>();
            this.refreshInfo = A.Fake<IRefreshInformation>();
            _principal = A.Fake<IPrincipal>();
            _context = A.Fake<HubCallerContext>();

            _fakeClient = A.Fake<ICiDashboardHub>();
            _fakeClients = A.Fake<IHubCallerConnectionContext<ICiDashboardHub>>();
        }

        [Test]
        public async Task OnConnect_QueriesUserProjectAndBuildConfigs_AndSendMappedInfoToClient_AndTriggersRefreshForThisClient()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var projects = _fixture.CreateMany<Project>().ToList();
            A.CallTo(() => this.infoQuery.GetUserProjectsAndBuildConfigs(username))
                .Returns(projects);

            A.CallTo(() => _fakeClients.Client(connectionId)).Returns(_fakeClient);

            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };
            hub.Clients = _fakeClients;
            await hub.OnConnected();

            A.CallTo(() => this.infoQuery.GetUserProjectsAndBuildConfigs(username))
                .MustHaveHappened();

            var buildCiIds = projects
                .SelectMany(
                    p => p.Builds
                        .Where(b => !string.IsNullOrEmpty(b.CiExternalId))
                        .ToList())
                .Select(b => b.CiExternalId)
                .ToList();

            A.CallTo(() => this.connectionsManager.AddBuildConfigs(connectionId,
                A<IEnumerable<BuildConfig>>.That.Matches(l => !l.Select(b => b.CiExternalId).Except(buildCiIds).Any())))
                .MustHaveHappened();

            A.CallTo(() => this.refreshInfo.SendRefreshBuildResults(connectionId))
                .MustHaveHappened();

            A.CallTo(() => _fakeClient
                    .SendProjectsAndBuildConfigs(projects))
                .MustHaveHappened();
        }

        [Test]
        public async Task OnDisconnected_RemovesUserBuilds()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };

            await hub.OnDisconnected(true);

            A.CallTo(() => this.connectionsManager.RemoveAllBuildConfigs(connectionId))
                .MustHaveHappened();
        }

        [Test]
        public async Task OnReconnected_QueriesUserProjectAndBuildConfigs_AndSendMappedInfoToClient()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var projects = _fixture.CreateMany<Project>().ToList();
            A.CallTo(() => this.infoQuery.GetUserProjectsAndBuildConfigs(username))
                .Returns(projects);

            A.CallTo(() => _fakeClients.Client(connectionId)).Returns(_fakeClient);
            
            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };
            hub.Clients = _fakeClients;
            await hub.OnReconnected();

            A.CallTo(() => this.infoQuery.GetUserProjectsAndBuildConfigs(username))
                .MustHaveHappened();

            var buildCiIds = projects
                .SelectMany(
                    p => p.Builds
                        .Where(b => !string.IsNullOrEmpty(b.CiExternalId))
                        .ToList())
                .Select(b => b.CiExternalId)
                .ToList();

            A.CallTo(() => this.connectionsManager.AddBuildConfigs(connectionId,
                A<IEnumerable<BuildConfig>>.That.Matches(l => !l.Select(b => b.CiExternalId).Except(buildCiIds).Any())))
                .MustHaveHappened();

            A.CallTo(() => _fakeClient
                    .SendProjectsAndBuildConfigs(projects))
                .MustHaveHappened();
        }

        [Test]
        public async Task RequestRefresh_ShouldCallsSendRefreshBuildResultsForConnectionIdOnly()
        {
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);

            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };

            await hub.RequestRefresh();

            A.CallTo(() => this.refreshInfo.SendRefreshBuildResults(connectionId))
                .MustHaveHappened();
        }

        [Test]
        public async Task RequestAllProjectBuilds_QueriesForAllProjectBuildConfigs_AndSendMappedInfoToClient()
        {
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);

            var builds = _fixture.CreateMany<BuildConfig>().ToList();
            A.CallTo(() => this.infoQuery.GetAllProjectBuildConfigs())
                .Returns(builds);

            A.CallTo(() => _fakeClients.Client(connectionId)).Returns(_fakeClient);

            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };
            hub.Clients = _fakeClients;
            await hub.RequestAllProjectBuilds();

            A.CallTo(() => this.infoQuery.GetAllProjectBuildConfigs())
                .MustHaveHappened();

            A.CallTo(() => _fakeClient
                    .SendProjectBuilds(builds))
                .MustHaveHappened();
        }

        [Test]
        public async Task AddNewProject_AddsNewProject_AndSendMappedInfoToClient()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var project = _fixture.Create<Project>();
            var resultProject = _fixture.Create<Project>();

            A.CallTo(() => this.commandProcessor.AddNewProject(username, project))
                .Returns(resultProject);

            A.CallTo(() => _fakeClients.Client(connectionId)).Returns(_fakeClient);

            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };
            hub.Clients = _fakeClients;
            await hub.AddNewProject(project);

            A.CallTo(() => this.commandProcessor.AddNewProject(username, project))
                .MustHaveHappened();

            A.CallTo(() => hub.Clients.Client(connectionId)
                    .SendUpdatedProject(A<ProjectUpdated>.That.Matches(p => p.OldId == project.Id && p.Project == resultProject)))
                .MustHaveHappened();
        }

        [Test]
        public async Task UpdateProjectName_UpdatesProject_AndSendFeedbackMessageToClient()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var project = _fixture.Create<Project>();

            A.CallTo(() => this.commandProcessor.UpdateProjectName(project.Id, project.Name))
                .Returns(true);

            A.CallTo(() => _fakeClients.Client(connectionId)).Returns(_fakeClient);
            
            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };
            hub.Clients = _fakeClients;
            await hub.UpdateProjectName(project.Id, project.Name);

            A.CallTo(() => this.commandProcessor.UpdateProjectName(project.Id, project.Name))
                .MustHaveHappened();

            A.CallTo(() => _fakeClient.SendMessage(
                A<FeedbackMessage>.That.Matches(p => p.Status == "Success" && p.Message == string.Format("Project {0} updated.", project.Name))))
                .MustHaveHappened();
        }

        [Test]
        public async Task RemoveProject_RemovesProject_AndUpdatesBuildsPerConnection_AndSendFeedbackMessageToClient()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var project = _fixture.Create<Project>();

            A.CallTo(() => this.commandProcessor.RemoveProject(project.Id))
                .Returns(project);

            A.CallTo(() => _fakeClients.Client(connectionId)).Returns(_fakeClient);
            
            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };
            hub.Clients = _fakeClients;
            await hub.RemoveProject(project.Id);

            A.CallTo(() => this.commandProcessor.RemoveProject(project.Id))
                .MustHaveHappened();

            foreach(var buildConfig in project.Builds)
            {
                A.CallTo(() => this.connectionsManager.RemoveBuildConfig(connectionId, buildConfig))
                    .MustHaveHappened();
            }

            A.CallTo(() => _fakeClient.SendMessage(
               A<FeedbackMessage>.That.Matches(p => p.Status == "Success" && p.Message == "Project removed")))
               .MustHaveHappened();
        }

        [Test]
        public async Task AddBuildToProject_AddBuild_AndUpdatesBuildsPerConnection_AndSendMappedInfoToClient()
        {
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);

            var project = _fixture.Create<Project>();

            var build = _fixture.Create<BuildConfig>();
            var newBuild = _fixture.Create<BuildConfig>();
            A.CallTo(() => this.commandProcessor.AddBuildConfigToProject(project.Id, build))
                .Returns(newBuild);

            A.CallTo(() => _fakeClients.Client(connectionId)).Returns(_fakeClient);

            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };
            hub.Clients = _fakeClients;
            await hub.AddBuildToProject(project.Id, build);

            A.CallTo(() => this.commandProcessor.AddBuildConfigToProject(project.Id, build))
                .MustHaveHappened();

            A.CallTo(() => this.connectionsManager.UpdateBuildConfigs(connectionId,
                A<IEnumerable<BuildConfig>>.That.Matches(l => l.First() == newBuild)))
                .MustHaveHappened();

            A.CallTo(() => _fakeClient
                    .SendUpdatedBuild(A<BuildConfigUpdated>.That.Matches(p => p.OldId == build.Id && p.Build == newBuild)))
                .MustHaveHappened();
        }

        [Test]
        public async Task RemoveBuildConfig_RemovesBuildConfig_AndUpdatesBuildsPerConnection_AndSendFeedbackMessageToClient()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var build = _fixture.Create<BuildConfig>();

            A.CallTo(() => this.commandProcessor.RemoveBuildConfig(build.Id))
                .Returns(build);

            A.CallTo(() => _fakeClients.Client(connectionId)).Returns(_fakeClient);

            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };
            hub.Clients = _fakeClients;
            await hub.RemoveBuild(build.Id);

            A.CallTo(() => this.commandProcessor.RemoveBuildConfig(build.Id))
                .MustHaveHappened();

            A.CallTo(() => this.connectionsManager.RemoveBuildConfig(connectionId, build))
                .MustHaveHappened();

            A.CallTo(() => _fakeClient.SendMessage(
               A<FeedbackMessage>.That.Matches(p => p.Status == "Success" && p.Message == "Build removed")))
               .MustHaveHappened();
        }

        [Test]
        public async Task UpdateBuildConfigExternalId_UpdatesBuildConfig_AndUpdatesBuildsPerConnection_AndSendFeedbackMessageToClient()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var build = _fixture.Create<BuildConfig>();

            A.CallTo(() => this.commandProcessor.UpdateBuildConfigExternalId(build.Id, build.Name, build.CiExternalId))
                .Returns(true);

            A.CallTo(() => _fakeClients.Client(connectionId)).Returns(_fakeClient);

            var hub = new CiDashboardHub(this.connectionsManager, this.commandProcessor, this.infoQuery, this.refreshInfo) { Context = _context };
            hub.Clients = _fakeClients;
            await hub.UpdateBuildConfigExternalId(build.Id, build.Name, build.CiExternalId);

            A.CallTo(() => this.commandProcessor.UpdateBuildConfigExternalId(build.Id, build.Name, build.CiExternalId))
                .MustHaveHappened();

            A.CallTo(() => this.connectionsManager.UpdateBuildConfigs(connectionId,
                A<IEnumerable<BuildConfig>>.That.Matches(l => l.First().CiExternalId == build.CiExternalId)))
                .MustHaveHappened();

            A.CallTo(() => _fakeClient.SendMessage(
                A<FeedbackMessage>.That.Matches(p => p.Status == "Success" && p.Message == string.Format("Build {0} updated.", build.Name))))
                .MustHaveHappened();
        }
    }
}
