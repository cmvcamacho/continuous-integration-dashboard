using System.Collections.Generic;

namespace CIDashboard.Web.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string User { get; set; }

        public int Order { get; set; }

        public virtual ICollection<BuildConfig> Builds { get; set; }
    }
}
