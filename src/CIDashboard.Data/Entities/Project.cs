using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CIDashboard.Data.Entities
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string User { get; set; }

        public int Order { get; set; }

        public virtual ICollection<Build> Builds { get; set; }
    }
}
