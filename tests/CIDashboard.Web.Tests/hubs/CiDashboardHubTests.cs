using System.Security.Principal;
using Autofac;
using CIDashboard.Data.Interfaces;
using CIDashboard.Web.hubs;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;
using Ploeh.AutoFixture.NUnit2;

namespace CIDashboard.Web.Tests.hubs
{
    [TestFixture]
    public class CiDashboardHubTests
    {
        private ILifetimeScope _lifetimeScope;
        private IPrincipal _principal;
        private HubCallerContext _context;
        private ICiDashboardService _ciDashboardService;
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            _lifetimeScope = A.Fake<ILifetimeScope>();
            _principal = A.Fake<IPrincipal>();
            _context = A.Fake<HubCallerContext>();

            _ciDashboardService = A.Fake<ICiDashboardService>();
            _fixture.Inject(_ciDashboardService);

        }

        [Test, AutoData]
        public void OnConnectStoresUserConnectionId(string connectionId)
        {
            var username = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var hub = new CiDashboardHub(_lifetimeScope) { Context = _context };

            hub.OnConnected();

            CiDashboardHub.ProjectsPerConnId.ContainsKey(connectionId).Should().BeTrue();
        }

        [Test]
        public void OnConnectStoresAndMaintainsUserConnectionIdOnSequentialAccesses()
        {
            var userName = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            A.CallTo(() => _principal.Identity.Name).Returns(userName);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);
            var hub = new CiDashboardHub(_lifetimeScope) { Context = _context };

            hub.OnConnected();

            var userName2 = _fixture.Create<string>();
            var connectionId2 = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(userName2);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId2);
            A.CallTo(() => _context.User).Returns(_principal);
         
            hub.OnConnected();

            CiDashboardHub.ProjectsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            CiDashboardHub.ProjectsPerConnId.ContainsKey(connectionId2).Should().BeTrue();
        }

        [Test, AutoData]
        public void OnReconnectedStoresUserConnectionId(string connectionId)
        {
            var username = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);

            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var hub = new CiDashboardHub(_lifetimeScope) { Context = _context };

            hub.OnReconnected();

            CiDashboardHub.ProjectsPerConnId.ContainsKey(connectionId).Should().BeTrue();
        }


        [Test, AutoData]
        public void OnDisconnectedRemovesUserConnectionId(string connectionId)
        {
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);

            var hub = new CiDashboardHub(_lifetimeScope) { Context = _context };

            hub.OnDisconnected(true);

            CiDashboardHub.ProjectsPerConnId.ContainsKey(connectionId).Should().BeFalse();
        }
    }
}
