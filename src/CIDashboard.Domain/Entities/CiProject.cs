using System.Collections.Generic;

namespace CIDashboard.Domain.Entities
{
    public class CiProject
    {
        public CiSource CiSource { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public IEnumerable<CiBuild> Builds { get; set; }
    }
}
