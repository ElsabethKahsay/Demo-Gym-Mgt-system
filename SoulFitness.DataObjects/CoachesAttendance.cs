using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects
{
    public class CoachesAttendance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public virtual Coaches Coach { get; set; }

        [ForeignKey("Coaches")]
        public int CoachId { get; set; }

        public string RFID { get; set; }
      
        public DateTime DateAndTime { get; set; }

    }
}
