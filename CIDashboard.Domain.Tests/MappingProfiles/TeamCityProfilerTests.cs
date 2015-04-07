using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CIDashboard.Domain.Entities;
using CIDashboard.Domain.MappingProfiles;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;
using TeamCitySharp.DomainEntities;

namespace CIDashboard.Domain.Tests.MappingProfiles
{
    [TestFixture]
    public class TeamCityProfilerTests
    {
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            this._fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization());
            this._fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            this._fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            
            Mapper.Initialize(cfg => cfg.AddProfile<TeamCityProfiler>());
        }

        [Test]
        public void MapsProjetsAndBuildTypesCorrectly()
        {
            var projects = _fixture
                .Build<Project>()
                .CreateMany();

            var mappedResult = Mapper.Map<IEnumerable<Project>, IEnumerable<CiProject>>(projects);

            var expectedResult = projects.Select(
                p => new CiProject
                {
                    CiSource = CiSource.TeamCity,
                    Id = p.Id,
                    Name = p.Name,
                    Url = p.WebUrl,
                    Builds = p.BuildTypes.BuildType.Select(
                        b => new CiBuild
                        {
                            CiSource = CiSource.TeamCity,
                            Id = b.Id,
                            Name = b.Name,
                            Url = b.WebUrl
                        })
                });

            mappedResult.ShouldBeEquivalentTo(expectedResult);
        }

        [TestCase("SUCCESS", CiBuildResultStatus.Success)]
        [TestCase("FAILURE", CiBuildResultStatus.Failure)]
        public void MapsBuildsCorrectly(string status, CiBuildResultStatus resultStatus)
        {
            var build = _fixture
                .Build<Build>()
                .With(b => b.Status, status)
                .Create();

            var mappedResult = Mapper.Map<Build, CiBuildResult>(build);

            var expectedResult = new CiBuildResult
                {
                    CiSource = CiSource.TeamCity,
                    Id = build.Id,
                    BuildId = build.BuildConfig.Id,
                    BuildName = build.BuildConfig.Name,
                    Url = build.WebUrl,
                    FinishDate = build.FinishDate,
                    StartDate = build.StartDate,
                    Version = build.Number,
                    Status = resultStatus
                };

            mappedResult.ShouldBeEquivalentTo(expectedResult);
        }
    }
}
