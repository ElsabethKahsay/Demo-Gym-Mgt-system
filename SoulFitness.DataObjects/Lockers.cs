using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects
{
    public class Locker:AuditTrail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int LockerNumber { get; set; }
        public bool IsAssigned { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public int Floor { get; set; }
        [Required]
        public LockersStatus LockersStatus { get; set; } 
    }
}