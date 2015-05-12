using System;

namespace CIDashboard.Web.Models
{
    public class BuildConfig
    {
        public int Id { get; set; }

        public string CiExternalId { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public string ProjectName { get; set; }
    }
}
