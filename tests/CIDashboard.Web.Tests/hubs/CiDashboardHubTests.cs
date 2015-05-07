using System.Security.Principal;
using System.Threading.Tasks;
using CIDashboard.Web.Hubs;
using CIDashboard.Web.Infrastructure;
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
        private IRefreshInformation _refreshInformation;
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            _refreshInformation = A.Fake<IRefreshInformation>();
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

            var hub = new CiDashboardHub(_refreshInformation) { Context = _context };

            await hub.OnConnected();

            A.CallTo(() => _refreshInformation.AddBuilds(username, connectionId))
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

            var hub = new CiDashboardHub(_refreshInformation) { Context = _context };

            await hub.OnReconnected();

            A.CallTo(() => _refreshInformation.AddBuilds(username, connectionId))
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

            var hub = new CiDashboardHub(_refreshInformation) { Context = _context };

            await hub.OnDisconnected(true);

            A.CallTo(() => _refreshInformation.RemoveBuilds(connectionId))
                .MustHaveHappened();
        }

        [Test]
        public async Task RequestRefreshCallsRefreshBuildsForConnectionIdOnly()
        {
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);

            var hub = new CiDashboardHub(_refreshInformation) { Context = _context };

            await hub.RequestRefresh();

            A.CallTo(() => _refreshInformation.RefreshBuilds(connectionId))
                .MustHaveHappened();
        }

        [Test]
        public async Task RequestRefreshCallsRequestAllProjectBuildsForConnectionIdOnly()
        {
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);

            var hub = new CiDashboardHub(_refreshInformation) { Context = _context };

            await hub.RequestAllProjectBuilds();

            A.CallTo(() => _refreshInformation.RequestAllProjectBuilds(connectionId))
                .MustHaveHappened();
        }
    }
}
