using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects
{
    public class CoachesSchedule:AuditTrail
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        public TimeSpan ScheduleStart { get; set; }       
        public TimeSpan ScheduleEnd { get; set; } 
        public virtual Coaches coach { get; set; }

        [ForeignKey("Coaches")]
        public int CoachId { get; set; }
        public DaysOfTheWeek Day { get; set; }
        

    }
}
