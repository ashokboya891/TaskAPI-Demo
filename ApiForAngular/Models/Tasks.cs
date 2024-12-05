
using ApiForAngular.ApplicationDbContext;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiForAngular.Models
{
    public class Tasks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ProjectID { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid AssignedTo { get; set; } // Change this to Guid
        public int TaskPriorityID { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string CurrentStatus { get; set; }
        public int CurrentTaskStatusID { get; set; }

        [NotMapped]
        public string CreatedOnString { get; set; }
        [NotMapped]
        public string LastUpdatedOnString { get; set; }

        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser CreatedByUser { get; set; }

        [ForeignKey("AssignedTo")]
        public virtual ApplicationUser AssignedToUser { get; set; }

        [ForeignKey("TaskPriorityID")]
        public virtual TaskPriority TaskPriority { get; set; }

        public virtual ICollection<TaskStatusDetail> TaskStatusDetails { get; set; }
    }

}

