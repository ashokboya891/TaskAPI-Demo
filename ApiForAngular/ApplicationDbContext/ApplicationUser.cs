using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiForAngular.ApplicationDbContext
{
    public class ApplicationUser: IdentityUser<Guid>
    {
       // public string? PersonName { set; get; }

        [NotMapped]
        public string Role { get; set; }
           
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int CountryID { get; set; }
        public bool ReceiveNewsLetters { get; set; }
    }

}
