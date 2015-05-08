using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
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

        [Test]
        public async Task GetProjectsReturnCorrectData()
        {
            _fixture.Customize<Build>(c => c.Without(f => f.Project));
            var projects = _fixture
                .CreateMany<Project>()
                .AsQueryable();
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

            var result = await service.GetProjects(username);

            result.Should()
                .NotBeNull()
                .And.NotBeEmpty()
                .And.BeEquivalentTo(projects.Where(p => p.User == username));
        }

        [Test]
        public async Task AddProjectReturnCorrectData()
        {
            _fixture.Customize<Build>(c => c.Without(f => f.Project));
            var projects = _fixture
                .CreateMany<Project>()
                .AsQueryable();
            var username = projects.First().User;

            var projectsSet = A.Fake<DbSet<Project>>(builder => builder
                .Implements(typeof(IQueryable<Project>))
                .Implements(typeof(IDbAsyncEnumerable<Project>)));

            var context = A.Fake<ICiDashboardContext>();
            A.CallTo(() => context.Projects).Returns(projectsSet);

            var factory = A.Fake<ICiDashboardContextFactory>();
            A.CallTo(() => factory.Create()).Returns(context);

            _fixture.Inject(factory);
            _fixture.Inject(context);

            var service = _fixture.Create<CiDashboardService>();

            var project = new Project { Name = "test" };
            await service.AddProject(username, project);

            A.CallTo(() => projectsSet.Add(A<Project>.That.Matches(p => p.Name == project.Name && p.User == username)))
                .MustHaveHappened();

            A.CallTo(() => context.SaveChanges())
                .MustHaveHappened();
        }
  
        [Test]
        public async Task UpdateProjectNameWithCorrectData()
        {
            _fixture.Customize<Build>(c => c.Without(f => f.Project));
            var projects = _fixture
                .CreateMany<Project>()
                .AsQueryable();
            var project = projects.First();

            var projectsSet = A.Fake<DbSet<Project>>(builder => builder
                .Implements(typeof(IQueryable<Project>))
                .Implements(typeof(IDbAsyncEnumerable<Project>)));
            A.CallTo(() => ((IDbAsyncEnumerable<Project>)projectsSet).GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<Project>(projects.GetEnumerator()));
            A.CallTo(() => ((IQueryable<Project>)projectsSet).Provider)
                .Returns(new TestDbAsyncQueryProvider<Project>(projects.Provider));
            A.CallTo(() => ((IQueryable<Project>)projectsSet).Expression).Returns(projects.Expression);
            A.CallTo(() => ((IQueryable<Project>)projectsSet).ElementType).Returns(projects.ElementType);
            A.CallTo(() => ((IQueryable<Project>)projectsSet).GetEnumerator()).Returns(projects.GetEnumerator());

            var context = A.Fake<ICiDashboardContext>();
            A.CallTo(() => context.Projects).Returns(projectsSet);

            var factory = A.Fake<ICiDashboardContextFactory>();
            A.CallTo(() => factory.Create()).Returns(context);

            _fixture.Inject(factory);
            _fixture.Inject(context);

            var service = _fixture.Create<CiDashboardService>();

            var newName = _fixture.Create<string>();
            var result = await service.UpdateProjectName(project.Id, newName);

            A.CallTo(() => context.SaveChanges())
                .MustHaveHappened();

            result.Should()
                .BeTrue();

            project.Name.Should()
                .BeEquivalentTo(newName);
        }

        [Test]
        public async Task UpdateProjectOrderWithCorrectData()
        {
            _fixture.Customize<Build>(c => c.Without(f => f.Project));
            var projects = _fixture
                .CreateMany<Project>()
                .AsQueryable();
            var project = projects.First();

            var projectsSet = A.Fake<DbSet<Project>>(builder => builder
                .Implements(typeof(IQueryable<Project>))
                .Implements(typeof(IDbAsyncEnumerable<Project>)));
            A.CallTo(() => ((IDbAsyncEnumerable<Project>)projectsSet).GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<Project>(projects.GetEnumerator()));
            A.CallTo(() => ((IQueryable<Project>)projectsSet).Provider)
                .Returns(new TestDbAsyncQueryProvider<Project>(projects.Provider));
            A.CallTo(() => ((IQueryable<Project>)projectsSet).Expression).Returns(projects.Expression);
            A.CallTo(() => ((IQueryable<Project>)projectsSet).ElementType).Returns(projects.ElementType);
            A.CallTo(() => ((IQueryable<Project>)projectsSet).GetEnumerator()).Returns(projects.GetEnumerator());

            var context = A.Fake<ICiDashboardContext>();
            A.CallTo(() => context.Projects).Returns(projectsSet);

            var factory = A.Fake<ICiDashboardContextFactory>();
            A.CallTo(() => factory.Create()).Returns(context);

            _fixture.Inject(factory);
            _fixture.Inject(context);

            var service = _fixture.Create<CiDashboardService>();

            var newPosition = _fixture.Create<int>();
            var result = await service.UpdateProjectOrder(project.Id, newPosition);

            A.CallTo(() => context.SaveChanges())
                .MustHaveHappened();

            result.Should()
                .BeTrue();

            project.Order
                .Should()
                .Be(newPosition);
        }

        [Test]
        public async Task RemoveProjectShouldRemoveTheProject()
        {
            _fixture.Customize<Build>(c => c.Without(f => f.Project));
            var projects = _fixture
                .CreateMany<Project>()
                .AsQueryable();
            var project = projects.First();

            var projectsSet = A.Fake<DbSet<Project>>(builder => builder
                .Implements(typeof(IQueryable<Project>))
                .Implements(typeof(IDbAsyncEnumerable<Project>)));
            A.CallTo(() => ((IDbAsyncEnumerable<Project>)projectsSet).GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<Project>(projects.GetEnumerator()));
            A.CallTo(() => ((IQueryable<Project>)projectsSet).Provider)
                .Returns(new TestDbAsyncQueryProvider<Project>(projects.Provider));
            A.CallTo(() => ((IQueryable<Project>)projectsSet).Expression).Returns(projects.Expression);
            A.CallTo(() => ((IQueryable<Project>)projectsSet).ElementType).Returns(projects.ElementType);
            A.CallTo(() => ((IQueryable<Project>)projectsSet).GetEnumerator()).Returns(projects.GetEnumerator());

            var context = A.Fake<ICiDashboardContext>();
            A.CallTo(() => context.Projects).Returns(projectsSet);

            var factory = A.Fake<ICiDashboardContextFactory>();
            A.CallTo(() => factory.Create()).Returns(context);

            _fixture.Inject(factory);
            _fixture.Inject(context);

            var service = _fixture.Create<CiDashboardService>();

            var result = await service.RemoveProject(project.Id);

            A.CallTo(() => context.SaveChanges())
                .MustHaveHappened();

            result.Should()
                .BeTrue();

            projectsSet.CountAsync().Result
                .Should()
                .Be(projects.Count() - 1);
        }

                [Test]
        public async Task AddBuildToProjectShouldAddTheBuildToProject()
        {
            _fixture.Customize<Build>(c => c.Without(f => f.Project));
            var projects = _fixture
                .CreateMany<Project>()
                .AsQueryable();
            var project = projects.First();

            var projectsSet = A.Fake<DbSet<Project>>(builder => builder
                .Implements(typeof(IQueryable<Project>))
                .Implements(typeof(IDbAsyncEnumerable<Project>)));
            A.CallTo(() => ((IDbAsyncEnumerable<Project>)projectsSet).GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<Project>(projects.GetEnumerator()));
            A.CallTo(() => ((IQueryable<Project>)projectsSet).Provider)
                .Returns(new TestDbAsyncQueryProvider<Project>(projects.Provider));
            A.CallTo(() => ((IQueryable<Project>)projectsSet).Expression).Returns(projects.Expression);
            A.CallTo(() => ((IQueryable<Project>)projectsSet).ElementType).Returns(projects.ElementType);
            A.CallTo(() => ((IQueryable<Project>)projectsSet).GetEnumerator()).Returns(projects.GetEnumerator());

            var context = A.Fake<ICiDashboardContext>();
            A.CallTo(() => context.Projects).Returns(projectsSet);

            var factory = A.Fake<ICiDashboardContextFactory>();
            A.CallTo(() => factory.Create()).Returns(context);

            _fixture.Inject(factory);
            _fixture.Inject(context);

            var build = _fixture
                .Create<Build>();

            var service = _fixture.Create<CiDashboardService>();

            var result = await service.AddBuildToProject(project.Id, build);

            A.CallTo(() => context.SaveChanges())
                .MustHaveHappened();

            result.Should()
                .NotBeNull();
        }
    }
}
