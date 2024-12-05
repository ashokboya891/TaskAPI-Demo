
using ApiForAngular.ApplicationDbContext;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Add this for [JsonIgnore]

namespace ApiForAngular.Models
{

    public class GroupedTask
    {
        public string TaskStatusName { get; set; }
        public List<Tasks> Tasks { get; set; }
    }
    public class Tasks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ProjectID { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid AssignedTo { get; set; }
        public int TaskPriorityID { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string? CurrentStatus { get; set; }
        public int CurrentTaskStatusID { get; set; }
        [NotMapped]
        public string? CreatedOnString { get; set; }
        [NotMapped]
        public string? LastUpdatedOnString { get; set; }

        public virtual Project? Project { get; set; }
        public virtual TaskPriority? TaskPriority { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }
        public virtual ApplicationUser? AssignedToUser { get; set; }

        // This is the navigation property that you need to check
        public virtual ICollection<TaskStatusDetail> TaskStatusDetails { get; set; } = new List<TaskStatusDetail>();
    }

    //[Key]
    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //public int TaskID { get; set; }
    //public string TaskName { get; set; }
    //public string? Description { get; set; }
    //public DateTime CreatedOn { get; set; }
    //public int ProjectID { get; set; }
    //public Guid CreatedBy { get; set; }
    //public Guid AssignedTo { get; set; }
    //public int TaskPriorityID { get; set; }
    //public DateTime LastUpdatedOn { get; set; }
    //public string? CurrentStatus { get; set; } // Make nullable
    //public int CurrentTaskStatusID { get; set; }
    //[NotMapped]
    //public string? CreatedOnString { get; set; } // Make nullable
    //[NotMapped]
    //public string? LastUpdatedOnString { get; set; } // Make nullable
    //public virtual Project? Project { get; set; }
    //public virtual TaskPriority? TaskPriority { get; set; }
    //public virtual ApplicationUser? CreatedByUser { get; set; }
    //public virtual ApplicationUser? AssignedToUser { get; set; }
    //public virtual ICollection<TaskStatusDetail> TaskStatusDetails { get; set; } = new List<TaskStatusDetail>();

    //public class Tasks
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public int TaskID { get; set; }
    //    public string TaskName { get; set; }
    //    public string Description { get; set; }
    //    public DateTime CreatedOn { get; set; }
    //    public int ProjectID { get; set; }
    //    public Guid CreatedBy { get; set; }
    //    public Guid AssignedTo { get; set; } // Change this to Guid
    //    public int TaskPriorityID { get; set; }
    //    public DateTime LastUpdatedOn { get; set; }
    //    public string CurrentStatus { get; set; }
    //    public int CurrentTaskStatusID { get; set; }

    //    [NotMapped]
    //    public string CreatedOnString { get; set; }
    //    [NotMapped]
    //    public string LastUpdatedOnString { get; set; }

    //    [ForeignKey("ProjectID")]
    //    public virtual Project Project { get; set; }

    //    [ForeignKey("CreatedBy")]
    //    public virtual ApplicationUser CreatedByUser { get; set; }

    //    [ForeignKey("AssignedTo")]
    //    public virtual ApplicationUser AssignedToUser { get; set; }

    //    [ForeignKey("TaskPriorityID")]
    //    public virtual TaskPriority TaskPriority { get; set; }

    //    public virtual ICollection<TaskStatusDetail> TaskStatusDetails { get; set; }
}
    //public class Tasks
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public int TaskID { get; set; }
    //    public string TaskName { get; set; }
    //    public string Description { get; set; }
    //    public DateTime CreatedOn { get; set; }
    //    public int ProjectID { get; set; }
    //    public Guid CreatedBy { get; set; }
    //    public Guid AssignedTo { get; set; }
    //    public int TaskPriorityID { get; set; }
    //    public DateTime LastUpdatedOn { get; set; }
    //    public string CurrentStatus { get; set; }
    //    public int CurrentTaskStatusID { get; set; }


    //    public string CreatedOnString { get; set; }

    //    public string LastUpdatedOnString { get; set; }

    //    // Make navigation properties nullable
    //    [ForeignKey("ProjectID")]
    //    [JsonIgnore]
    //    public virtual Project? Project { get; set; }

    //    [ForeignKey("CreatedBy")]
    //    [JsonIgnore]
    //    public virtual ApplicationUser? CreatedByUser { get; set; }

    //    [ForeignKey("AssignedTo")]
    //    [JsonIgnore]
    //    public virtual ApplicationUser? AssignedToUser { get; set; }

    //    [ForeignKey("TaskPriorityID")]
    //    [JsonIgnore]
    //    public virtual TaskPriority? TaskPriority { get; set; }

    //    [JsonIgnore]
    //    public virtual ICollection<TaskStatusDetail>? TaskStatusDetails { get; set; }
    //}

    //public class Tasks
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public int TaskID { get; set; }
    //    public string TaskName { get; set; }
    //    public string Description { get; set; }
    //    public DateTime CreatedOn { get; set; }
    //    public int ProjectID { get; set; }
    //    public Guid CreatedBy { get; set; }
    //    public Guid AssignedTo { get; set; } // Change this to Guid
    //    public int TaskPriorityID { get; set; }
    //    public DateTime LastUpdatedOn { get; set; }
    //    public string CurrentStatus { get; set; }
    //    public int CurrentTaskStatusID { get; set; }

    //    [NotMapped]
    //    public string CreatedOnString { get; set; }
    //    [NotMapped]
    //    public string LastUpdatedOnString { get; set; }

    //    [ForeignKey("ProjectID")]
    //    public virtual Project Project { get; set; }

    //    [ForeignKey("CreatedBy")]
    //    public virtual ApplicationUser CreatedByUser { get; set; }

    //    [ForeignKey("AssignedTo")]
    //    public virtual ApplicationUser AssignedToUser { get; set; }

    //    [ForeignKey("TaskPriorityID")]
    //    public virtual TaskPriority TaskPriority { get; set; }

    //    public virtual ICollection<TaskStatusDetail> TaskStatusDetails { get; set; }
    //}



