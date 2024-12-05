using ApiForAngular.ApplicationDbContext;
using ApiForAngular.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiForAngular.Models
{
    using System.Text.Json.Serialization; // Add this for [JsonIgnore]

    public class TaskStatusDetail
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskStatusDetailID { get; set; }
        public int TaskID { get; set; }
        public int TaskStatusID { get; set; }
        public Guid UserID { get; set; }
        public string Description { get; set; }
        public DateTime StatusUpdationDateTime { get; set; }
        [NotMapped]
        public string StatusUpdationDateTimeString { get; set; }

        // Foreign Key relationships
        [ForeignKey("TaskStatusID")]
        public virtual TaskStatuses TaskStatus { get; set; }
        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; }


        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int TaskStatusDetailID { get; set; }
        //public int TaskID { get; set; }
        //public int TaskStatusID { get; set; }
        //public Guid UserID { get; set; }
        //public string Description { get; set; }
        //public DateTime StatusUpdationDateTime { get; set; }

        //[NotMapped]
        //public string StatusUpdationDateTimeString { get; set; }

        //[ForeignKey("TaskStatusID")]
        //[JsonIgnore]
        //public virtual TaskStatus? TaskStatus { get; set; }

        //[ForeignKey("UserID")]
        //[JsonIgnore]
        //public virtual ApplicationUser? User { get; set; }
    }

    //public class TaskStatusDetail
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public int TaskStatusDetailID { get; set; }
    //    public int TaskID { get; set; }
    //    public int TaskStatusID { get; set; }
    //    public Guid UserID { get; set; } // Change this to Guid
    //    public string Description { get; set; }
    //    public DateTime StatusUpdationDateTime { get; set; }
    //    [NotMapped]
    //    public string StatusUpdationDateTimeString { get; set; }

    //    [ForeignKey("TaskStatusID")]
    //    public virtual TaskStatuses TaskStatus { get; set; }

    //    [ForeignKey("UserID")]
    //    public virtual ApplicationUser User { get; set; }
    //}

}



