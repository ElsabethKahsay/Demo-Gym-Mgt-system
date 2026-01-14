using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects
{
    public class Coaches:AuditTrail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id {  get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [NotMapped]
        public string FMName { get { return string.Format("{0} {1}", FirstName, LastName); } }
        [Required]
        public int EmployeeId {  get; set; }
        [Required]
        public Gender Gender { get; set; }
        public string RFID { get; set; }
        [Required]
        public string MobileNo { get; set; }
        [Required]
        public Status Status { get; set; }
    }
}
