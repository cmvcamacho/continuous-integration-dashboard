using System;

namespace CIDashboard.Web.Models
{
    public class Build
    {
        public int Id { get; set; }

        public string CiExternalId { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Status { get; set; }

        public string Url { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime FinishDate { get; set; }

        public int NumberTestPassed { get; set; } //PassedTestCount

        public int NumberTestIgnored { get; set; } //IgnoredTestCount

        public int NumberTestFailed { get; set; } //FailedTestCount

        public int NumberTestTotal
        {
            get
            {
                return this.NumberTestPassed + this.NumberTestFailed + this.NumberTestIgnored;
            }
        }

        public int NumberStatementsCovered { get; set; } //CodeCoverageAbsSCovered

        public int NumberStatementsTotal { get; set; } //CodeCoverageAbsSTotal

        public double CodeCoverage
        {
            get
            {
                return this.NumberStatementsTotal == 0
                    ? 0
                    : Math.Round(
                        (this.NumberStatementsCovered / (double)this.NumberStatementsTotal) * 100,
                        2);
            }
        }

    }
}
