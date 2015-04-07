using System.Collections.Generic;

namespace CIDashboard.Web.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string User { get; set; }

        public int Order { get; set; }

        public int NoBuild
        {
            get
            {
                return this.Builds == null
                    ? 0
                    : this.Builds.Count;
            }
        }

        public virtual ICollection<Build> Builds { get; set; }
    }
}