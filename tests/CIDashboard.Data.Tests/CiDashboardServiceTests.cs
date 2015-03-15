using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Autofac;
using Autofac.Extras.FakeItEasy;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;
using Ploeh.AutoFixture.NUnit2;

namespace CIDashboard.Data.Tests
{
    [TestFixture]
    public class CiDashboardServiceTests
    {
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());
        }

        [Test, AutoData]
        public void GetProjectsReturnCorrectData()
        {
            var projects = _fixture.CreateMany<Project>().AsQueryable();
            var username = projects.First().User;

            var projectsSet = A.Fake<DbSet<Project>>(builder => builder
                .Implements(typeof (IQueryable<Project>))
                .Implements(typeof(IDbAsyncEnumerable<Project>)));
            A.CallTo(() => ((IDbAsyncEnumerable<Project>)projectsSet).GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<Project>(projects.GetEnumerator()));
            A.CallTo(() => ((IQueryable<Project>)projectsSet).Provider)
                .Returns(new TestDbAsyncQueryProvider<Project>(projects.Provider));
            A.CallTo(() => ((IQueryable<Project>) projectsSet).Expression).Returns(projects.Expression);
            A.CallTo(() => ((IQueryable<Project>) projectsSet).ElementType).Returns(projects.ElementType);
            A.CallTo(() => ((IQueryable<Project>)projectsSet).GetEnumerator()).Returns(projects.GetEnumerator());


            var context = A.Fake<ICiDashboardContext>();
            A.CallTo(() => context.Projects).Returns(projectsSet);

            var factory = A.Fake<ICiDashboardContextFactory>();
            A.CallTo(() => factory.Create()).Returns(context);

            _fixture.Inject(factory);
            _fixture.Inject(context);

            var service = _fixture.Create<CiDashboardService>();

            var result = service.GetProjects(username).Result;
            result
                .Should()
                .NotBeNull()
                .And.NotBeEmpty()
                .And.BeEquivalentTo(projects.Where(p => p.User == username));
        }
    }
}
