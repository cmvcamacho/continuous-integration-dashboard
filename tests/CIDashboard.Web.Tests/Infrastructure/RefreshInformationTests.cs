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
        public void AddBuildsQueryForProjectsWhenUserIsNew()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();

            refresh.AddBuilds(username, connectionId);

            RefreshInformation.ProjectsPerConnId.ContainsKey(connectionId).Should().BeTrue();
        }

        [Test]
        public void AddBuildsDontQueryForProjectsWhenUserAlreadyConnected()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();
            refresh.AddBuilds(username, connectionId);
            refresh.AddBuilds(username, connectionId);

            Assert.Inconclusive("not done yet....");
            //RefreshInformation.ProjectsPerConnId.ContainsKey(connectionId).Should().BeFalse();
        }

        [Test]
        public void RemoveBuildsRemovesProjectsForConnectionId()
        {
            var username = _fixture.Create<string>();
            var connectionId = _fixture.Create<string>();

            var refresh = new RefreshInformation();
            refresh.AddBuilds(username, connectionId);
            RefreshInformation.ProjectsPerConnId.ContainsKey(connectionId).Should().BeTrue();

            refresh.RemoveBuilds(connectionId);
            RefreshInformation.ProjectsPerConnId.ContainsKey(connectionId).Should().BeFalse();
        }
    }
}
