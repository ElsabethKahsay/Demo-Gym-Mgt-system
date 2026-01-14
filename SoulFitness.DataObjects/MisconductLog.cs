using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SoulFitness.DataObjects
{
    public class MisconductLog:AuditTrail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string MisconductType { get; set; }
        [Required]
        public string Notes { get;set; }
        [Required]
        public MisconductStatus MisconductStatus {  get; set; } 
        public int FineAmount {  get; set; }

    }
}
