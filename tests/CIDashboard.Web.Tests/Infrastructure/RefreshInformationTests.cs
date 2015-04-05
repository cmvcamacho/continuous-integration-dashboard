using System.Threading.Tasks;
using CIDashboard.Web.Infrastructure;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

namespace CIDashboard.Web.Tests.Infrastructure
{
    [TestFixture]
    public class RefreshInformationTests
    {
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());
        }

        [Test]
        public async Task AddBuildsQueryForProjectsWhenUserIsNew()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();

            await refresh.AddBuilds(username, connectionId);

            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
        }

        [Test]
        public async Task AddBuildsAddsOnlyNewBuilds()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();

            refresh.AddBuilds(username, connectionId);

            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();
            Assert.Inconclusive("not done yet....");
        }

        [Test]
        public async Task AddBuildsDontQueryForProjectsWhenUserAlreadyConnected()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();
            refresh.AddBuilds(username, connectionId);
            refresh.AddBuilds(username, connectionId);

            Assert.Inconclusive("not done yet....");
            //RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeFalse();
        }

        [Test]
        public async Task RemoveBuildsRemovesBuildsForConnectionId()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();
            refresh.AddBuilds(username, connectionId);
            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();

            refresh.RemoveBuilds(connectionId);
            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeFalse();
        }

        [Test]
        public async Task RemoveBuildsDontRemoveBuildWhenOtherUsersAreUsingIt()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();
            refresh.AddBuilds(username, connectionId);
            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeTrue();

            refresh.RemoveBuilds(connectionId);
            RefreshInformation.BuildsPerConnId.ContainsKey(connectionId).Should().BeFalse();

            Assert.Inconclusive("not done yet....");

        }
    }
}
