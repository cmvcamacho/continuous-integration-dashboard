using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using CIDashboard.Data.Entities;
using CIDashboard.Data.Interfaces;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;

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
        public async Task GetProjects_ReturnCorrectData()
        {
            _fixture.Customize<BuildConfig>(c => c.Without(f => f.Project));
            var projects = _fixture
                .CreateMany<Project>()
                .AsQueryable();
            var username = projects.First().User;

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

            var result = await service.GetProjects(username);

            result.Should()
                .NotBeNull()
                .And.NotBeEmpty()
                .And.BeEquivalentTo(projects.Where(p => p.User == username));
        }

        [Test]
        public async Task AddProject_ReturnCorrectData()
        {
            _fixture.Customize<BuildConfig>(c => c.Without(f => f.Project));
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
        public async Task UpdateProjectName_ReturnsProjectUpdatedWithNewName()
        {
            _fixture.Customize<BuildConfig>(c => c.Without(f => f.Project));
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
                .Be(newName);
        }

        [Test]
        public async Task UpdateProjectOrder_ReturnsProjectUpdatedWithNewOrder()
        {
            _fixture.Customize<BuildConfig>(c => c.Without(f => f.Project));
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
        public async Task RemoveProject_ShouldRemoveTheProjectFromDatastore_AndReturnRemovedProject()
        {
            _fixture.Customize<BuildConfig>(c => c.Without(f => f.Project));
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

            A.CallTo(() => projectsSet.Remove(A<Project>.That.Matches(p => p.Id == project.Id)))
                .MustHaveHappened();

            result.Should()
                .NotBeNull()
                .And.BeSameAs(project);
        }

        [Test]
        public async Task AddBuildConfigToProject_ShouldAddTheNewBuildConfigToProject()
        {
            _fixture.Customize<BuildConfig>(c => c.Without(f => f.Project));
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
                .Create<BuildConfig>();

            var service = _fixture.Create<CiDashboardService>();

            var result = await service.AddBuildConfigToProject(project.Id, build);

            A.CallTo(() => context.SaveChanges())
                .MustHaveHappened();

            result.Should()
                .NotBeNull();
        }

        [Test]
        public async Task RemoveBuildConfig_ShouldRemoveTheBuildConfigFromDatastore_AndReturnRemovedBuildConfig()
        {
            _fixture.Customize<BuildConfig>(c => c.Without(f => f.Project));
            var builds = _fixture
                .CreateMany<BuildConfig>()
                .AsQueryable();
            var build = builds.First();

            var buildsSet = A.Fake<DbSet<BuildConfig>>(builder => builder
                .Implements(typeof(IQueryable<BuildConfig>))
                .Implements(typeof(IDbAsyncEnumerable<BuildConfig>)));
            A.CallTo(() => ((IDbAsyncEnumerable<BuildConfig>)buildsSet).GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<BuildConfig>(builds.GetEnumerator()));
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).Provider)
                .Returns(new TestDbAsyncQueryProvider<BuildConfig>(builds.Provider));
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).Expression).Returns(builds.Expression);
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).ElementType).Returns(builds.ElementType);
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).GetEnumerator()).Returns(builds.GetEnumerator());

            var context = A.Fake<ICiDashboardContext>();
            A.CallTo(() => context.BuildConfigs).Returns(buildsSet);

            var factory = A.Fake<ICiDashboardContextFactory>();
            A.CallTo(() => factory.Create()).Returns(context);

            _fixture.Inject(factory);
            _fixture.Inject(context);

            var service = _fixture.Create<CiDashboardService>();

            var result = await service.RemoveBuildConfig(build.Id);

            A.CallTo(() => context.SaveChanges())
                .MustHaveHappened();

            A.CallTo(() => buildsSet.Remove(A<BuildConfig>.That.Matches(p => p.Id == build.Id)))
                .MustHaveHappened();

            result.Should()
                .NotBeNull()
                .And.BeSameAs(build);
        }

        [Test]
        public async Task UpdateBuildConfigCiExternalId_ReturnsBuildConfigUpdatedWithNewNameAndCiExternalId()
        {
            _fixture.Customize<BuildConfig>(c => c.Without(f => f.Project));
            var builds = _fixture
                .CreateMany<BuildConfig>()
                .AsQueryable();
            var build = builds.First();

            var buildsSet = A.Fake<DbSet<BuildConfig>>(builder => builder
                .Implements(typeof(IQueryable<BuildConfig>))
                .Implements(typeof(IDbAsyncEnumerable<BuildConfig>)));
            A.CallTo(() => ((IDbAsyncEnumerable<BuildConfig>)buildsSet).GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<BuildConfig>(builds.GetEnumerator()));
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).Provider)
                .Returns(new TestDbAsyncQueryProvider<BuildConfig>(builds.Provider));
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).Expression).Returns(builds.Expression);
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).ElementType).Returns(builds.ElementType);
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).GetEnumerator()).Returns(builds.GetEnumerator());

            var context = A.Fake<ICiDashboardContext>();
            A.CallTo(() => context.BuildConfigs).Returns(buildsSet);

            var factory = A.Fake<ICiDashboardContextFactory>();
            A.CallTo(() => factory.Create()).Returns(context);

            _fixture.Inject(factory);
            _fixture.Inject(context);

            var service = _fixture.Create<CiDashboardService>();

            var newName = _fixture.Create<string>();
            var newCiExternalId = _fixture.Create<string>();
            var result = await service.UpdateBuildConfigExternalId(build.Id, newName, newCiExternalId);

            A.CallTo(() => context.SaveChanges())
                .MustHaveHappened();

            result.Should()
                .BeTrue();

            build.Name.Should()
                .Be(newName);
            build.CiExternalId.Should()
                .Be(newCiExternalId);
        }

        [Test]
        public async Task UpdateBuildconfigOrder_ReturnsBuildconfigUpdatedWithNewOrder()
        {
            _fixture.Customize<BuildConfig>(c => c.Without(f => f.Project));
            var builds = _fixture
                .CreateMany<BuildConfig>()
                .AsQueryable();
            var build = builds.First();

            var buildsSet = A.Fake<DbSet<BuildConfig>>(builder => builder
                .Implements(typeof(IQueryable<BuildConfig>))
                .Implements(typeof(IDbAsyncEnumerable<BuildConfig>)));
            A.CallTo(() => ((IDbAsyncEnumerable<BuildConfig>)buildsSet).GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<BuildConfig>(builds.GetEnumerator()));
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).Provider)
                .Returns(new TestDbAsyncQueryProvider<BuildConfig>(builds.Provider));
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).Expression).Returns(builds.Expression);
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).ElementType).Returns(builds.ElementType);
            A.CallTo(() => ((IQueryable<BuildConfig>)buildsSet).GetEnumerator()).Returns(builds.GetEnumerator());

            var context = A.Fake<ICiDashboardContext>();
            A.CallTo(() => context.BuildConfigs).Returns(buildsSet);

            var factory = A.Fake<ICiDashboardContextFactory>();
            A.CallTo(() => factory.Create()).Returns(context);

            _fixture.Inject(factory);
            _fixture.Inject(context);

            var service = _fixture.Create<CiDashboardService>();

            var newPosition = _fixture.Create<int>();
            var result = await service.UpdateBuildConfigOrder(build.Id, newPosition);

            A.CallTo(() => context.SaveChanges())
                .MustHaveHappened();

            result.Should()
                .BeTrue();

            build.Order
                .Should()
                .Be(newPosition);
        }
    }
}
