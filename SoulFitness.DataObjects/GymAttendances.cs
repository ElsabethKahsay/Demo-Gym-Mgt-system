using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFitness.DataObjects
{
    // This class is model for the GymAttendance table in the database to keep 
    // track of daily activities 
    // Relates with the Employees table
    public class GymAttendances
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public virtual Employee Employees { get; set; }

        [ForeignKey("Employees")]
        public long EmpID { get; set; }

        public string RFID { get; set; }
        public int LockerNumber { get; set; }
        public int FloorNumber { get; set; }
        public bool IsReturned { get; set; }

        public DateTime DateAndTime { get; set; }
        public DateTime ReturnDateAndTime { get; set; }

        public Status AttendanceStatus { get; set; }



    }
}
