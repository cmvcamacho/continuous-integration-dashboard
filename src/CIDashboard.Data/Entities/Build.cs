using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIDashboard.Data.Entities
{
    public class Build
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public string CiExternalId { get; set; }

        public int Order { get; set; }

        public virtual Project Project { get; set; }
    }
}
