using ApiForAngular.ApplicationDbContext;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Skill
{
    [Key]
    public int SkillID { get; set; }

    public string SkillName { get; set; }
    public string SkillLevel { get; set; }

    // Change the Id to Guid to match the primary key of ApplicationUser
    public Guid ApplicationUserId { get; set; }  // Foreign key for ApplicationUser

    //[ForeignKey("ApplicationUserId")]  // Specify the foreign key property
    //public virtual ApplicationUser ApplicationUser { get; set; }
}
