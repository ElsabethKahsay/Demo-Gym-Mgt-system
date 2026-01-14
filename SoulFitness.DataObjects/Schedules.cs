using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects
{
    // This class is model for the Schedule table in the database to keep 
    // track of gym schedules
    
    public class Schedules : AuditTrail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "TimeInterval is required")]
        public string TimeInterval { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Limit is required")]
        public int Limit { get; set; }

        public Status status { get; set; }

    }
}