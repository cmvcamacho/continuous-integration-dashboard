using System;

namespace CIDashboard.Domain.Entities
{
    public class Build
    {
        public string BuildId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public string Url { get; set; }
        public bool Success { get; set; }

        public int NumberTestPassed { get; set; } //PassedTestCount
        public int NumberTestIgnored { get; set; } //IgnoredTestCount
        public int NumberTestFailed { get; set; } //FailedTestCount
        public int NumberTestTotal
        {
            get { return this.NumberTestPassed + this.NumberTestFailed + this.NumberTestIgnored; }
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
                        (this.NumberStatementsCovered/(double) this.NumberStatementsTotal)*100, 2);
            }
        }

    }
}
