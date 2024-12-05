using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiForAngular.Models
{
    public class ClientLocations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int ClientLocationID { get; set; }
        public string ClientLocationName { get; set; }
    }
}
