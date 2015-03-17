using System.Security.Principal;
using CIDashboard.Web.Hubs;
using CIDashboard.Web.Infrastructure;
using FakeItEasy;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
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
        private IBackgroundJobClient _backgroundJobClient;
        private IRefreshInformation _refreshInformation;
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());

            _backgroundJobClient = A.Fake<IBackgroundJobClient>();
            _refreshInformation = A.Fake<IRefreshInformation>();
            _principal = A.Fake<IPrincipal>();
            _context = A.Fake<HubCallerContext>();
        }

        [Test]
        public void OnConnectEnqueueAddBuildsMethodToHangfire()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var hub = new CiDashboardHub(_backgroundJobClient, _refreshInformation) { Context = _context };

            hub.OnConnected();

            A.CallTo(() => _backgroundJobClient.Create(
                    A<Job>.That.Matches(job => job.Method.Name == "AddBuilds" && job.Arguments[0].Contains(username) && job.Arguments[1].Contains(connectionId)),
                    A<EnqueuedState>._))
                .MustHaveHappened();
        }

        [Test]
        public void OnReconnectedEnqueueAddBuildsMethodToHangfire()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var hub = new CiDashboardHub(_backgroundJobClient, _refreshInformation) { Context = _context };

            hub.OnReconnected();

            A.CallTo(() => _backgroundJobClient.Create(
                    A<Job>.That.Matches(job => job.Method.Name == "AddBuilds" && job.Arguments[0].Contains(username) && job.Arguments[1].Contains(connectionId)),
                    A<EnqueuedState>._))
                .MustHaveHappened();
        }


        [Test]
        public void OnDisconnectedEnqueueRemoveBuildsMethodToHangfire()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();
            A.CallTo(() => _principal.Identity.Name).Returns(username);
            A.CallTo(() => _context.ConnectionId).Returns(connectionId);
            A.CallTo(() => _context.User).Returns(_principal);

            var hub = new CiDashboardHub(_backgroundJobClient, _refreshInformation) { Context = _context };

            hub.OnDisconnected(true);

            A.CallTo(() => _backgroundJobClient.Create(
                    A<Job>.That.Matches(job => job.Method.Name == "RemoveBuilds" && job.Arguments[0].Contains(connectionId)),
                    A<EnqueuedState>._))
                .MustHaveHappened();
        }
    }
}
